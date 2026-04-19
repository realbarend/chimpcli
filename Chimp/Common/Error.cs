namespace Chimp.Common;

public class Error(string message, Exception? inner, params object?[] args) : Exception(message, inner)
{
    public object?[] Args { get; } = args;

    public Error(string message, params object?[] args) : this(message, null, args) { }
}
