using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chimp.Api;

public static class SerializeExtensions
{
    public static string Serialize(this object data) => JsonSerializer.Serialize(data, SerializerOptions);

    public static T Deserialize<T>(this string json, bool strict = true)
        => JsonSerializer.Deserialize<T>(json, strict ? StrictSerializerOptions : SerializerOptions)
           ?? throw new ApplicationException($"deserialization of {typeof(T)} failed");

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static readonly JsonSerializerOptions StrictSerializerOptions = new(SerializerOptions)
    {
        // detect early when the chimp devs decide to change their models
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };
}
