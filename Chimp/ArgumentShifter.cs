namespace Chimp;

public class ArgumentShifter(string[] args)
{
    private int _pointer;

    public string GetString(string paramName, string? defaultValue = null)
    {
        if (_pointer < args.Length) return args[_pointer++];

        if (defaultValue != null) return defaultValue;
        throw new PebcakException($"cannot read '{paramName}': ran out of args");
    }

    public int GetInt32(string paramName, string? defaultValue = null)
    {
        var str = GetString(paramName, defaultValue);
        if (int.TryParse(str, out var value)) return value;
        throw new PebcakException($"cannot read '{paramName}': cannot parse number");
    }

    public string[] GetRemainingArgs()
    {
        var result = args[_pointer..];
        _pointer = args.Length;
        return result;
    }
}