namespace EnvironmentErrors;

public class EnvironmentVariableNotFound : Exception
{

    public EnvironmentVariableNotFound(string message) : base(message) { }
}
