using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Chimp.Api;
using Chimp.Api.Models;
using Chimp.DomainModels;
using Chimp.Services;

namespace Chimp;

// Json Source Generation removes the use of reflection.
// This allows us to use trimming (PublishTrimmed).

[JsonSerializable(typeof(UserRecord))]
[JsonSerializable(typeof(List<ProjectRecord>))]
[JsonSerializable(typeof(List<ProjectTaskRecord>))]
[JsonSerializable(typeof(TagRecord[]))]
[JsonSerializable(typeof(List<TimeSheetRecord>))]
[JsonSerializable(typeof(TimeSheetRecord))]
[JsonSerializable(typeof(NewTimeSheetRecord))]
[JsonSerializable(typeof(ProjectTask[]))]
[JsonSerializable(typeof(Tag[]))]
[JsonSerializable(typeof(TimeSheetRowDto))]
[JsonSerializable(typeof(TimeSheetService.ApiCredentials))]
[JsonSerializable(typeof(TimeSheetService.TimeSheetDate))]
[JsonSerializable(typeof(CognitoAuthentication.Credentials))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true)]
internal partial class JsonContext : JsonSerializerContext
{
    public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, (JsonTypeInfo<T>)Default.GetTypeInfo(typeof(T))!);

    public static T? Deserialize<T>(string json)
        => JsonSerializer.Deserialize(json, (JsonTypeInfo<T>)Default.GetTypeInfo(typeof(T))!);

    public static JsonElement SerializeToElement<T>(T obj)
        => JsonSerializer.SerializeToElement(obj, (JsonTypeInfo<T>)Default.GetTypeInfo(typeof(T))!);

    public static T? DeserializeFromElement<T>(JsonElement element)
        => element.Deserialize((JsonTypeInfo<T>)Default.GetTypeInfo(typeof(T))!);
}
