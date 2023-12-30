namespace Chimp;

public class ArgumentShifter(string[] args)
{
    private int _pointer;

    public string GetString(string paramName, string? defaultValue = null)
    {
        if (_pointer < args.Length) return args[_pointer++];

        if (defaultValue != null) return defaultValue;
        throw new PebcakException("expected '{ParamName}' parameter missing", new() {{"ParamName", paramName}});
    }

    public int GetInt32(string paramName, string? defaultValue = null)
    {
        var str = GetString(paramName, defaultValue);
        if (int.TryParse(str, out var value)) return value;
        throw new PebcakException("parameter '{ParamName}' must be a number", new() {{"ParamName", paramName}});
    }

    public string[] GetRemainingArgs()
    {
        var result = args[_pointer..];
        _pointer = args.Length;
        return result;
    }
}