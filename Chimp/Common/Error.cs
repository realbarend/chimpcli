namespace Chimp.Common;

public class Error(string message, params object?[] args) : Exception(message)
{
    public object?[] Args { get; } = args;
}
