using Npgsql;
using System.Data;
using Auth;

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

    protected internal ConnectionState ConnectionState => connection.State;
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

    public async Task<User?> CreateUserAsync(string email, string hashedPassword)
    {
        string sql = "INSERT INTO users (id, created_at, updated_at, email, hashed_password, is_chirpy_red) VALUES (gen_random_uuid(), NOW(), NOW(), @Email, @Password, false) RETURNING *;";

        using (var command = new NpgsqlCommand(sql, connection))
        {
            // Safely adding parameter to prevent SQL injection
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@Password", hashedPassword);

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
                        HashedPassword = reader.GetString(reader.GetOrdinal("email")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
                        isChirpyRed = reader.GetBoolean(reader.GetOrdinal("is_chirpy_red"))
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
                    Id = reader.GetGuid(reader.GetOrdinal("id")),  
                    Body = reader.GetString(reader.GetOrdinal("body")),
                    UserId = reader.GetGuid(reader.GetOrdinal("user_id")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"))
                };
            }
    }
    return null;
}

    public async Task<List<ChirpResponse>> GetAllChirps(string sort, string? author = null)
    {
        var sql = (author, sort) switch {
            (null, "asc")       => "SELECT * FROM chirps ORDER BY created_at",
            (null, "desc")      => "SELECT * from chirps order by created_at DESC",
            (_, "desc")         => "SELECT * FROM chirps WHERE user_id = @author ORDER BY created_at DESC",
            (_,_)              => "SELECT * FROM chirps WHERE user_id = @author ORDER BY created_at"
        };

        var chirps = new List<ChirpResponse>();
        using (var command = new NpgsqlCommand(sql, connection))
        {

            if (author != null) {
                command.Parameters.AddWithValue("@author", Guid.Parse(author));
            }

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
    }


    public async Task<ChirpResponse?> GetAChirp(Guid ChirpID)
    {
        string sql = "SELECT * FROM chirps WHERE id = @id";

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

    public async Task<LoginResponse?> LoginUser(string email, string password) 
    {
        string sql = "SELECT * FROM users WHERE email = @Email";

        using (var command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@Email", email);
            using (var reader = await command.ExecuteReaderAsync())
                if (await reader.ReadAsync()){
                    var dbpw = reader.GetString(reader.GetOrdinal("hashed_password"));
                    if (PasswordGenerator.CheckPassword(password, dbpw)) {
                        return new LoginResponse {
                            Id = reader.GetGuid(reader.GetOrdinal("id")),
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
                            IsChirpyRed = reader.GetBoolean(reader.GetOrdinal("is_chirpy_red"))

                        }; 
                    }
                }
        }
        return null;
    }
    public async Task<RefreshToken?> CreateRefreshToken(Guid userCredentials, string refreshToken, DateTime refreshTokenExpiration){
       
        string sql = "INSERT INTO refresh_tokens (token, created_at, updated_at, user_id, expires_at, revoked_at) VALUES (@token, NOW(), NOW(), @id, @refreshTokenExpiration, NULL) RETURNING *;";

        using (var command = new NpgsqlCommand(sql, connection)) {
            command.Parameters.AddWithValue("@id", userCredentials);
            command.Parameters.AddWithValue("@token", refreshToken);
            command.Parameters.AddWithValue("@refreshTokenExpiration", refreshTokenExpiration);
            using (var reader = await command.ExecuteReaderAsync())
                if (await reader.ReadAsync()) {
                    return new RefreshToken {
                        UserId = reader.GetGuid("user_id"),
                        Token = reader.GetString("token"),
                        CreatedAt = reader.GetDateTime("created_at"),
                        RevokedAt = reader.IsDBNull(reader.GetOrdinal("revoked_at")) 
                           ? null 
                           : reader.GetDateTime("revoked_at"),
                        ExpiresAt = reader.GetDateTime("expires_at"),
                        UpdatedAt = reader.GetDateTime("updated_at"),
                    };
                }
        }

        return null;
    }

    public async Task<RefreshToken?> RefreshToken(string refreshToken) 
    {
        string sql = @"SELECT user_id, token, created_at, revoked_at, expires_at, updated_at 
            FROM refresh_tokens WHERE token = @rt";
        RefreshToken? tokenData = null;

        using (var command = new NpgsqlCommand(sql, connection)) 
        {
            command.Parameters.AddWithValue("@rt", refreshToken);
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync()) 
                {
                    tokenData = new RefreshToken 
                    {
                        UserId = reader.GetGuid("user_id"),
                           Token = reader.GetString("token"),
                           CreatedAt = reader.GetDateTime("created_at"),
                           RevokedAt = reader.IsDBNull(reader.GetOrdinal("revoked_at")) 
                               ? null 
                               : reader.GetDateTime("revoked_at"),
                           ExpiresAt = reader.GetDateTime("expires_at"),
                           UpdatedAt = reader.GetDateTime("updated_at")
                    };
                }
            }
        }

        if (tokenData?.ExpiresAt <= DateTime.UtcNow || tokenData?.RevokedAt != null) {
            return null;
        }

        return tokenData;
    }

    public async Task<RefreshToken?> FindToken(string refreshToken) {
        string sql = @"SELECT * FROM refresh_tokens WHERE token = @rt";
        RefreshToken? tokenData = null;
        using (var command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@rt", refreshToken);
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                    tokenData = new RefreshToken 
                    {
                        UserId = reader.GetGuid("user_id"),
                               Token = reader.GetString("token"),
                               CreatedAt = reader.GetDateTime("created_at"),
                               RevokedAt = reader.IsDBNull(reader.GetOrdinal("revoked_at")) 
                                   ? null 
                                   : reader.GetDateTime("revoked_at"),
                               ExpiresAt = reader.GetDateTime("expires_at"),
                               UpdatedAt = reader.GetDateTime("updated_at")
                    };

                if (tokenData == null || tokenData.ExpiresAt <= DateTime.UtcNow || tokenData.RevokedAt != null){
                    return null;
                }
            }

        }
        return tokenData;
    }
    
    public async Task<bool> RevokeToken(string refreshToken) {
        string sql = @"UPDATE refresh_tokens SET updated_at = @updateTime, revoked_at = @revokeTime WHERE token = @rt";

        using (var command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@updateTime", DateTime.UtcNow);
            command.Parameters.AddWithValue("@revokeTime", DateTime.UtcNow);
            command.Parameters.AddWithValue("@rt", refreshToken);
            int Affected = await command.ExecuteNonQueryAsync();
            return Affected == 1;
        }
    }

    public async Task<EmailAndPassword?> UpdateUserInfo(string email, string password, Guid userID) 
    {
        string sql = @"UPDATE users 
            SET email = @email, hashed_password = @hashed_password 
            WHERE id = @userID 
            RETURNING email";

        using (var command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@hashed_password", password);
            command.Parameters.AddWithValue("@userID", userID);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    return new EmailAndPassword { Email = email };
                }
            }
        }
        return null;
    }
    public async Task<bool> DeleteAChirp(Guid chirpId){
        
        string sql = @"DELETE FROM chirps WHERE id = @chirpId";
        using (var command = new NpgsqlCommand(sql, connection)){
            command.Parameters.AddWithValue("@chirpId", chirpId);
            int affected = await command.ExecuteNonQueryAsync();
            return affected == 1;
        }
    }

    public async Task<bool> UpgradeToChirpyRed(Guid UserID) {
        string sql = @"UPDATE users SET is_chirpy_red = true WHERE id = @UserID";
        using (var command = new NpgsqlCommand(sql, connection)){
            command.Parameters.AddWithValue("@UserID", UserID);
            int affected = await command.ExecuteNonQueryAsync();
            return affected == 1;
        }
    }
}
