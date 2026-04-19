using System.Text.Json;
using Chimp.Common;

namespace Chimp.Api;

public static class SerializeExtensions
{
    public static string Serialize<T>(this T data) => JsonContext.Serialize(data);

    public static T Deserialize<T>(this string json)
    {
        try
        {
            return JsonContext.Deserialize<T>(json) ?? throw new ApplicationException($"deserialization of {typeof(T)} failed");
        }
        catch (JsonException e) when (e.Message.Contains("could not be mapped"))
        {
            throw new Error("the TimeChimp API returned an unrecognized field. The API may have been updated. In that case, the error can only be fixed by updating TimeChimp CLI to a newer version.", e);
        }
    }
}
