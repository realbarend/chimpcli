namespace Chimp.Api;

public static class SerializeExtensions
{
    public static string Serialize<T>(this T data) => JsonContext.Serialize(data);

    public static T Deserialize<T>(this string json)
        => JsonContext.Deserialize<T>(json) ?? throw new ApplicationException($"deserialization of {typeof(T)} failed");
}
