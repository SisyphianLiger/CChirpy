# Jason...Jay Sun?, JJ Abrams? No it's JSON

Ok, now we are actually coding a server. But how can we...the server makers, in all of our glory, process Json?

It's actually quite simple! Well, sort of, first we actually get to use our Models folder remember them! Let me give you a quick refresher of the folders in C#.

```go
    Controllers/
    ├── AdminController.cs
    ├── ChirpyController.cs
    ├── CustomStaticFileController.cs
    Middleware/
    ├── MetricsMiddleware.cs
    Models/
    ├── Chirp.cs
    ├── ChirpResponse.cs
    ├── JsonError.cs
    Properties/
    Services/
    ├── ChirpValidator.cs
    wwwroot/
    ├── assets/
    │   └── logo.png
    ├── index.html
```

As you can see two new additions have made there way! Services, and Models, which was there before actually but has finally been used to handle the data. Now to the Json!!! What does a C# do to handle the Json??

Lets first take a look at what a chirp would look like.
```cs
using System.Text.Json.Serialization;

public class Chirp
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }
}
```

Pretty simple, we make sure we add a serialization package, which takes care of most of the overhead of processing Json. Then we create a class with a property and annotate on the top what the key from the json KV pair will be, in our case "body". 

What about Go's version? Go is a bit less complicated and more compact in my opinion. Let's take a look
```go
	type parameters struct {
		Body string `json:"body"`
	}
```

now to be clear the naming convention is due to this func being within a handler call. In other words, we just assume a handler recieves some body. But as can be seen the tagging of `json:body` is equivalent to `[JsonPropertyName("body")]` no stress there.

## What do our handlers now look like?
Well with our data established, what is missing is an implementation of an interface that validates and cleans a body. I decided to make an interface again, that would be implemented into a class called ChirpValidator, which would do two similar things, validate a chirp, and clean the chirp for potential bad language. Now, the supposed bad language was made by the assignment but I hope you can see here, what is going on. 

```cs
public interface IChirpValidator
{
    bool isValid(string chirp);
    string cleanedBody(string? chirp);
}

public class ChirpValidator : IChirpValidator
{

    private List<string> listOfBannedWords = new List<string> 
    { 
        "kerfuffle", 
        "sharbert", 
        "fornax" 
    };

    public bool isValid(string chirp)
    {
        return chirp.Length <= 140;
    }

    public string cleanedBody(string? chirp) {

        if (chirp == null) {
            return "";
        }

        var data = chirp.Split(" ")
                        .Select(word => listOfBannedWords.Contains(word.ToLower()) ? "****" : word);

        return string.Join(" ", data);
    }
}
```

So, just like our Middleware we implement two functions isValid and cleanedBody, that take in chirps, in cleaned body, as we will see, it has the possibility of being a null value. Json response doesn't always contain a body after all. That is where we see the maybe operator with ?. I also figured out the FP equivalent of Map(), which is called Select in C#. 

So I was able to easily create a string array from my initial string, that would take a copy, replacing bad words with "****", and then joining them again. 

Ok, looks pretty good, but before we see how its implemented in out API, spoiler alert, there is another dependency injection. Let's take a look at the Go code.

```go

func handlerChirpsValidate(w http.ResponseWriter, r *http.Request) {
	type parameters struct {
		Body string `json:"body"`
	}
	type returnVals struct {
		CleanedBody string `json:"cleaned_body"`
	}

	decoder := json.NewDecoder(r.Body)
	params := parameters{}
	err := decoder.Decode(&params)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, "Couldn't decode parameters", err)
		return
	}

	const maxChirpLength = 140
	if len(params.Body) > maxChirpLength {
		respondWithError(w, http.StatusBadRequest, "Chirp is too long", nil)
		return
	}

	badWords := map[string]struct{}{
		"kerfuffle": {},
		"sharbert":  {},
		"fornax":    {},
	}
	cleaned := getCleanedBody(params.Body, badWords)

	respondWithJSON(w, http.StatusOK, returnVals{
		CleanedBody: cleaned,
	})
}

func getCleanedBody(body string, badWords map[string]struct{}) string {
	words := strings.Split(body, " ")
	for i, word := range words {
		loweredWord := strings.ToLower(word)
		if _, ok := badWords[loweredWord]; ok {
			words[i] = "****"
		}
	}
	cleaned := strings.Join(words, " ")
	return cleaned
}
```

The go code could definitely use a refactor, currently there is a lot of logic that should probably be in another place. But that will happen later. For now we can just see the main differnces between the handlers. Interestingly enough, instead of explaining the code in detail, as it is equivalent to the C# code, I would much rather talk about what respondWithJSON is doing here. 

Lets view it!
```go
func respondWithJSON(w http.ResponseWriter, code int, payload interface{}) {
	w.Header().Set("Content-Type", "application/json")
	dat, err := json.Marshal(payload)
	if err != nil {
		log.Printf("Error marshalling JSON: %s", err)
		w.WriteHeader(500)
		return
	}
	w.WriteHeader(code)
	w.Write(dat)
}
```

as we can see here, `respondWithJson` takes in a ResponseWriter, status code, and payload, which is an empty interface, otherwise can be thought of as a generic, and it then sets a Json response with information it receives within a handler. This is a pretty clever way to handle various Responses. The funny part is while this code is quite nice, the C# package for .NET have options to already handle the header and payload of a particular response.

So lets take a look!

## C\# is returning JaySawn
```cs

    [HttpPost("validate_chirp")]
    public ActionResult ValidateChirp([FromBody] Chirp? request) =>
        request switch {
                { Body: null }                                          => BadRequest( new JsonError { Error = "Body is required" }),
                { Body: var paylod }  when !_validator.isValid(paylod)  => BadRequest( new JsonError { Error = "Chirp is too long" }),
                _                                                       =>  Ok(new ChirpResponse { CleanedBody = _validator.cleanedBody(request?.Body) }),
        };
```

Oh yea, did I forget to mention C\# has pattern matching...because well yea, it does and I love pattern matching, so what are we doing here? Well just like in the code above we check the following potential issues wrapping them in either BadRequest or Ok, if BadRequest, the status returns 400, Ok is 200, and therefore we can just return a new instance of our instantiated classes, i.e. JsonError, and ChirpResponse. 

I have to admit, C\# has the upper hand here for me, but it also interops with more FP paradigms. Still, I thought this was a fun jump into Json, and seeing the diffrences between the two langs really help me understand that besides the organization the data is arranged relatively simmilar.

## Conclusion
Well, I have my Json now set up, and that is great! but where are we going to sent the Json too...I think a DB will have to work, so when I am back writing again, it will be for a DB.

