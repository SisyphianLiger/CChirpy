# Oh the troubles I have seen...

Database's can be a tricky beast, and with this section, that much is true. First, let me begin to explain the problem I faced while completing this section. The main issue, C# likes to promote their very fancy and well done Entity Service. But...this is an Orm and orm is one letter away from worm, and I don't like worms in my code, especially when they start talking with other bugs. 

Sorry side tracked there, this was the longest portion of the module, so I wanted to joke. Anyways, what does a SQL enjoyer, such as myself, do in times of dire Orm environments. Writes Raw SQL of course. Now, I want to note, I did not sanitize my inputs, I would obviously do this in production, but this was about seeing the differences between C# and Golang, so no need to add more complexity.

There is a two step process to making a DB successful, the first is can I connect to a DB, this is actually a rather streamlined process for both C# and Golang, The second, can I migrate my DB, DB's afterall have tables, and with great tables, come great BNF, and the like. The answer to that question is yes, and using DbUp, a dependency within the C# ecosystem, I was still able to keep my project relatively dependency free (looking at you entity), while continuing to learn about Backend Webdev. 

So lets take a look at the process of connecting a DB to a API in your Backend.

## The Process, a High level overview

I was once interviewed for a job, that I was not fully ready for yet. In that interview, there was talks of dependency injection and how we set up 3rd party API's. This was great, and I still think of this information everytime I code. What was the info? Well to put it simply, when you are setting up an API, even though it would make most sense to write that API intuitively, you should actually think and seperate the logic of say, 3rd Party API respones, intermediary middleware and finally your API call. So that stated, I will now show you one example of this, that I think highlights the best qualities of this module, while also keeping this reading section lightweight yet informative. 

## The Making of Create A Chirp
I will be explaining the process of how I made a connection to the DB in C# then Go, then I will explain the way sqlc looks, and the way my raw SQL in C# looks. In C# I also use `Npgsql` that handles adding the variables to the string that will be a SQL query. Then we will see the differences in queries, mainly dealing with the async of C# because otherwise its all SQL at the end of the day, and lastly the API Handler that uses the DB call. 

### Connecting to a DB
In C#...for whatever reason, dotenv is not really well practiced. I found an article however writing about [dotenv](https://dusted.codes/dotenv-in-dotnet) in dotnet. There was some custom code I used to allow for a .env file to exist. That being stated its otherwise a pretty simple utility. 

```cs

    public static void Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new EnvironmentVariableNotFound("No Availble file from path:" + filePath + "\n");
        }

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split("=", count: 2, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                continue;
            }

            Environment.SetEnvironmentVariable(parts[0], parts[1]);
        }
    }
```

There is a class name but I feel that it would be silly to add in, basically, this takes in the filepath and splits the file into an environment variable dictionary that has a key value pair. This by itself, is somewhat useless as I have classes that require different .env information, so I made a Utilities class called Configuration

```cs

public class ConfigurationAccess
{

    public string DbUrl { get; private set; }
    public bool DevMode { get; private set; }
    public int LocalHost { get; private set; }
    public bool MigrateDB { get; private set; }

    public ConfigurationAccess()
    {
        LoadVariablesFromEnv();
        DbUrl = CheckForValidEnvVariable<string>("DB_URL");
        DevMode = CheckForValidEnvVariable<bool>("DEV");
        LocalHost = CheckForValidEnvVariable<int>("LOCALHOST");
        MigrateDB = CheckForValidEnvVariable<bool>("MIGRATE_DATABASE");

    }

```

It should be pretty straightforward what I am doing here, but basically, I just want the information coming in to be easily accessible. So I load the variables in, a function that calles the function from previous and loadem up! Then I store them into a config class which then passes them down to repective classes such as the DB. But, and this is what I want to highlight, while doing this process I wanted to make it such that the process of desciphering what the EnvVar was would be Generic. Because there are multiple different types of EnvVar and well no reason on being restricted when we have `T`. 


```cs 
    private T CheckForValidEnvVariable<T>(string EnvVar)
    {

        var value = Environment.GetEnvironmentVariable(EnvVar);


        if (string.IsNullOrWhiteSpace(value))
        {
            throw new EnvironmentVariableNotFound($"Environment variable `{EnvVar}` not found or is empty");
        }

        value = value.Trim('"');

        try
        {
            if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(value, out var result))
                {
                    return (T)(object)result;
                }
                throw new InvalidCastException($"Unable to convert `{value}` to `{typeof(T)}`");
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception)
        {
            throw new InvalidCastException($"Unable to convert `{value}` to `{typeof(T)}`");
        }

    }
}
```

And that is just what I did here, now there is a caveat that this function would not work for DateTime, but there is a simple fix, which is adding a TryParse for that. I just decided that until I needed to add Datetime, I would only add function specifications for what mattered. Interesetingly enough I find that being able to case a type T into an Object a very cool idea. Also, with this could I have been introduced to the out keyword.

### Golangs version
Golang was far simpler, for starters, there is a dotenv package, that you can just use. 
```go
func main() {
	const filepathRoot = "."
	const port = "8080"

	godotenv.Load()
	dbURL := os.Getenv("DB_URL")
	if dbURL == "" {
		log.Fatal("DB_URL must be set")
	}
    // code continues

```

This means less extra classes, andwell you can just access them based on "NAME_CONVENTION". A much much nicer fit. So yea, Golang wins this round! And will perhaps, as we will see, continue to win.

## The DB function
In C# we need to do three things within our DB function, passdown the connection string, make the SQL query as a string, and then append the item that needs to be swapped using the library discussed previously, and finally if the query is successful, we return a in memory object that matches the information found from the query. 

That is a mouthful but lets just take a look at `CreateChirp`.

First C#
```cs

    public async Task<ChirpResponse?> CreateChirp(string body, Guid user_id)
    {
        string sql = "INSERT INTO chirps (id, created_at, updated_at, body, user_id) VALUES (gen_random_uuid(), NOW(), NOW(), @body, @user_id) RETURNING *;";

        using (var command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@body", body);
            command.Parameters.AddWithValue("@user_id", user_id);
            using (var reader = await command.ExecuteReaderAsync())
                if (await reader.ReadAsync())
                {
                    return new ChirpResponse
                    {
                        Body = reader.GetString(reader.GetOrdinal("body")),
                        Id = reader.GetGuid(reader.GetOrdinal("user_id"))
                    };
                }
        }

        return null;
    }
```

Now to better understand this we can break down the following code into steps. First, we have a body and user_id that is used in the post, we will need to make a query that posts, i.e. Inserts into chirps our table. This will be done by connecting to Nqgsql as a command, then we swap the values where there are @ signs within the string with the values coming in. (This is where we sanitize I know). And then, we execute a command to read from the database, and when we have read, we can then return a response that matches the return body for the client. If there is something that goes amuck, we say there is no object returned, i.e. null.

Pretty straightforward, but definitely not as nice as Golangs, in my opinion. Let's check out Golangs version. First, we write the sql in is query form with the following annotations.

```sql
-- name: CreateChirp :one
INSERT INTO chirps (id, created_at, updated_at, body, user_id)
VALUES (
    gen_random_uuid(),
    NOW(),
    NOW(),
    $1,
    $2
)
RETURNING *;
```

So first we right the query, now unlike C#, its really nice that you can write sql within your golang project. Then running sqlc, it generates the golang code for you.

```Go
func (q *Queries) CreateChirp(ctx context.Context, arg CreateChirpParams) (Chirp, error) {
	row := q.db.QueryRowContext(ctx, createChirp, arg.Body, arg.UserID)
	var i Chirp
	err := row.Scan(
		&i.ID,
		&i.CreatedAt,
		&i.UpdatedAt,
		&i.Body,
		&i.UserID,
	)
	return i, err
}
```

You can check the code if you want, but its autogenerated, and pretty easy to see.

And that is it! One Query, and we are ready to go!

## The API Call

Finally we get to the API call, lets check out C#'s
```cs
    [HttpPost]
    public async Task<IActionResult> CreateChirp([FromBody] ChirpRequest chirp)
    {
        if (chirp.Body == null)
        {
            return await Task.FromResult(StatusCode(StatusCodes.Status400BadRequest, new JsonError { Error = "Invalid Body, cannot send chirp" }));
        }
        if (!_validator.isValid(chirp.Body))
        {
            return await Task.FromResult(StatusCode(StatusCodes.Status400BadRequest, new JsonError { Error = "Chirp is too long" }));
        }

        var chirpIt = await _chirpydb.CreateChirp(_validator.cleanedBody(chirp.Body), chirp.Id);

        return await Task.FromResult(StatusCode(StatusCodes.Status201Created, new ChirpResponse { Id = chirp.Id, Body = _validator.cleanedBody(chirp.Body) }));
    }
```

So, with a post request we will take in a body, then check if that body is null, if so we return it as a bad request, or if the body is not null but is invalid based on our functions from previous we can then return a 400. Pretty simple, also, kind of like Golang we return classes that hold fields for Json, called a JsonError here. its a custom data type I made in my models section. And also, if we create it! Then we need a 201 and return a Chirpresponse!

It's important to note that the Task here makes the return type available for async. That is why in every return we await the Result from task.

Here are the models used for this Handler.

```cs


public class JsonError
{

    [JsonPropertyName("error")]
    public string? Error { get; set; }

}

public class ChirpResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }
}

```
It should be noted, that we don't need to set all the data with ChirpResponse.


Now then golang!!!
```Go
func (cfg *apiConfig) handlerChirpsCreate(w http.ResponseWriter, r *http.Request) {
	type parameters struct {
		Body   string    `json:"body"`
		UserID uuid.UUID `json:"user_id"`
	}

	decoder := json.NewDecoder(r.Body)
	params := parameters{}
	err := decoder.Decode(&params)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, "Couldn't decode parameters", err)
		return
	}

	cleaned, err := validateChirp(params.Body)
	if err != nil {
		respondWithError(w, http.StatusBadRequest, err.Error(), err)
		return
	}

	chirp, err := cfg.db.CreateChirp(r.Context(), database.CreateChirpParams{
		Body:   cleaned,
		UserID: params.UserID,
	})
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, "Couldn't create chirp", err)
		return
	}

	respondWithJSON(w, http.StatusCreated, Chirp{
		ID:        chirp.ID,
		CreatedAt: chirp.CreatedAt,
		UpdatedAt: chirp.UpdatedAt,
		Body:      chirp.Body,
		UserID:    chirp.UserID,
	})
}
```

Ok, so at first glance Golangs is a bit heavier, but that is because of the following. In C#, there if a [FromBody] annotation which takes care of the decoding function, then we obviously clean up the function, and response with the proper json struct. So, more lines of code, but all the same steps.


## Conclusion

That was my tour of making a endpoint to a DB. I have to say I much prefer Golang here, both because Sqlc is really nice and allows queries to be written but also, just from the simple setup and execution of both. Now then, my stuff is VERY insecure, so in the next section, we are going to implement our own Auth!
