namespace Chimp;

public abstract class LocalizedException(string message, Dictionary<string, object>? args = null) : Exception(message)
{
    public Dictionary<string, object>? Args { get; } = args;
}
