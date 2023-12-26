using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chimp;

public static class Util
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static readonly JsonSerializerOptions StrictJsonSerializerOptions = new(JsonSerializerOptions)
    {
        // detect early when the chimp devs decide to change their models
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    public static string JsonSerialize(object data) => JsonSerializer.Serialize(data, JsonSerializerOptions);

    public static T JsonDeserialize<T>(string json, bool strict = true)
        => JsonSerializer.Deserialize<T>(json, strict ? StrictJsonSerializerOptions : JsonSerializerOptions)
           ?? throw new ApplicationException($"deserialization of {typeof(T)} failed");

    public static DateTime GetFirstDayOfWeek(DateTime baseDate) => baseDate.AddDays(-FindDayOffsetFromMonday(baseDate)).Date;

    private static int FindDayOffsetFromMonday(DateTime baseDate)
    {
        var daysFromMonday = 0;
        for (var date = baseDate.Date; date.DayOfWeek != DayOfWeek.Monday; date = date.AddDays(-1)) daysFromMonday++;
        return daysFromMonday;
    }

    public static string HoursNotation(double hours)
    {
        var whole = (int)hours;
        return (hours - whole) switch
        {
            // may the rounding gods have mercy
            0    => $"{whole}'00",
            0.25 => $"{whole}'15",
            0.5  => $"{whole}'30",
            0.75 => $"{whole}'45",
            _ => hours.ToString(CultureInfo.InvariantCulture)
        };
    }

    public static Cookie? GetCookie(this HttpResponseMessage response, Uri uri, string cookieName)
    {
        var cookies = new CookieContainer();
        foreach (var cookieHeader in response.Headers.GetValues("Set-Cookie")) cookies.SetCookies(uri, cookieHeader);
        return cookies.GetCookies(uri).FirstOrDefault(c => c.Name == cookieName);
    }
}