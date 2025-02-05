using PostgresDB;
using EnvironmentErrors;

namespace Utilities;


public class ConfigurationAccess
{

    public string DbUrl { get; private set; }
    public bool DevMode { get; private set; }
    public int LocalHost { get; private set; }
    public bool MigrateDB { get; private set; }
    public string JwtSecret { get; private set; }
    public string PolkaKey { get; private set; }

    public ConfigurationAccess()
    {
        LoadVariablesFromEnv();
        DbUrl = CheckForValidEnvVariable<string>("DB_URL");
        DevMode = CheckForValidEnvVariable<bool>("DEV");
        LocalHost = CheckForValidEnvVariable<int>("LOCALHOST");
        MigrateDB = CheckForValidEnvVariable<bool>("MIGRATE_DATABASE");
        JwtSecret = CheckForValidEnvVariable<string>("JWT_SECRET");
        PolkaKey = CheckForValidEnvVariable<string>("POLKA_KEY");

    }

    private void LoadVariablesFromEnv()
    {
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, ".env");
        DotEnv.Load(dotenv);
    }

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
