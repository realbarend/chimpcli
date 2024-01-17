using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Chimp.Models;

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

    public static bool TryParseProjectSpec(string projectSpec, out int project, out int[] tags)
    {
        // project match: p1 or p1-1 or p1-1,2

        project = 0;
        tags = Array.Empty<int>();
        var projectMatch = Regex.Match(projectSpec, @"^p(?<Project>\d{1,2}(?:-\d{1,2}(?:,\d{1,2})*)?)$");
        if (!projectMatch.Success) return false;

        var strProject = projectMatch.Groups["Project"].Value;
        if (strProject.Contains('-'))
        {
            tags = strProject[(strProject.IndexOf('-') + 1)..].Split(',').Select(int.Parse).ToArray();
            strProject = strProject[..strProject.IndexOf('-')];
        }

        project = int.Parse(strProject);
        return true;
    }

    public static Cookie? GetCookie(this HttpResponseMessage response, Uri uri, string cookieName)
    {
        var cookies = new CookieContainer();
        foreach (var cookieHeader in response.Headers.GetValues("Set-Cookie"))
        {
            try
            {
                cookies.SetCookies(uri, cookieHeader);
            }
            catch (CookieException) { /* this happens when the cookieheader is not valid */ }
        }
        return cookies.GetCookies(uri).FirstOrDefault(c => c.Name == cookieName);
    }
    
    public static string? Truncate(this string? s, int length) => s == null || s.Length < length ? s : s[..length];

    public static ProjectViewModel GetProjectByLine(this List<ProjectViewModel> projects, int line)
    {
        return projects.SingleOrDefault(p => p.Line == line)
            ?? throw new PebcakException("previously fetched projects list does not contain line #{Line}", new() {{"Line",line}});
    }

    public static List<long> MapTagLinesToIds(this List<TagViewModel> tags, IEnumerable<int> lines)
    {
        return lines.Select(tagLine => tags.SingleOrDefault(t => t.Line == tagLine)?.ApiTag.Id
                                ?? throw new PebcakException("previously fetched tags list does not contain line #{Line}", new() {{"Line", tagLine}}))
            .ToList();
    }
}