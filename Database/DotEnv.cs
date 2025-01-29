using EnvironmentErrors;

namespace PostgresDB;


public static class DotEnv
{
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
}
