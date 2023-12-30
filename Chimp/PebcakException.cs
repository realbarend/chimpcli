namespace Chimp;

public class PebcakException(string message, Dictionary<string, object>? args = null) : LocalizedException(message, args);
