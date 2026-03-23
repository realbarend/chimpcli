namespace Chimp.Common;

public class Error(string message, object? args = null) : Exception(message)
{
    public object? Args { get; } = args;
}
