using Npgsql;

namespace PostgresDB;

public class ChirpyDatabase
{

    protected readonly string connection_string;
    protected readonly NpgsqlConnection connection;
    public bool DevDB { get; private set; }

    public ChirpyDatabase(string path, bool DevMode)
    {
        connection_string = path;
        connection = new NpgsqlConnection(path);
        DevDB = DevMode;

    }

    public async Task OpenAsync()
    {
        if (connection.State == System.Data.ConnectionState.Closed)
        {
            await connection.OpenAsync();
            Console.WriteLine("Connection to Postgres DB Opened!");
        }
    }

    public void EndConnectionToDB()
    {
        if (connection != null && connection.State != System.Data.ConnectionState.Closed)
        {
            connection.Close();
        }
    }

    public async Task<User?> CreateUserAsync(string email)
    {
        string sql = "INSERT INTO users (id, created_at, updated_at, email) VALUES (gen_random_uuid(), NOW(), NOW(), @Email) RETURNING *;";

        using (var command = new NpgsqlCommand(sql, connection))
        {
            // Safely adding parameter to prevent SQL injection
            command.Parameters.AddWithValue("@Email", email);

            // Executing the command and processing the result
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    // Map the database row to your User class (if you have one)
                    return new User
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("id")),
                        Email = reader.GetString(reader.GetOrdinal("email")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
                    };
                }
            }
        }

        // Return null if insertion fails
        return null;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        string sql = "DELETE FROM users WHERE id = @id";

        using (var command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@Id", id);

            try
            {

                return await command.ExecuteNonQueryAsync() == 1;

            }
            catch
            {
                return false;
            }
        }
    }

    public async Task<bool> DeleteAllUsers()
    {

        string sql = "DELETE FROM users";

        using (var command = new NpgsqlCommand(sql, connection))
        {
            try
            {
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

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

    public async Task<List<ChirpResponse>> GetAllChirps()
    {
        string sql = "SELECT * FROM chirps ORDER BY created_at";
        var chirps = new List<ChirpResponse>();

        using (var command = new NpgsqlCommand(sql, connection))

        using (var reader = await command.ExecuteReaderAsync())
            while (await reader.ReadAsync())
            {
                var chirp = new ChirpResponse
                {
                    Id = reader.GetGuid(reader.GetOrdinal("id")),
                    Body = reader.GetString(reader.GetOrdinal("body")),
                    UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
                };
                chirps.Add(chirp);

            }
        return chirps;
    }


    public async Task<ChirpResponse?> GetAChirp(Guid ChirpID)
    {
        string sql = "SELECT * FROM chirps WHERE user_id = @id";

        using (var command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@id", ChirpID);
            using (var reader = await command.ExecuteReaderAsync())
                if (await reader.ReadAsync())
                    return new ChirpResponse
                    {
                        Id = reader.GetGuid(reader.GetOrdinal("id")),
                        Body = reader.GetString(reader.GetOrdinal("body")),
                        UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
                    };
        }
        return null;
    }
}
