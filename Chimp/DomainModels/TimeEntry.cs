using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Chimp.Common;

namespace Chimp.DomainModels;

public record TimeEntry
{
    public TimeOnly? StartTime { get; init; }
    public TimeSpan Duration { get; init; }
    public DayOfWeek? DayOfWeek { get; init; }

    public static bool TryParse(string input, [NotNullWhen(true)] out TimeEntry? timeEntry)
    {
        // supported format: "ww:<time>" where ww is first two letters of the weekday, e.g. 'mo' for Monday
        var timeEntryStr = input;
        if (TryParseDayPrefix(timeEntryStr, out var dayOfWeek))
        {
            // strip weekday so we can proceed processsing the time-part
            timeEntryStr = timeEntryStr[(timeEntryStr.IndexOf(':')+1)..];
        }

        // supported formats: "14.30+15" or "14+30" or "930+60" or "9:30+30"
        if ( ! TryParseStartTimePlusMinutes(timeEntryStr, out timeEntry)
             // supported formats: "14.30-15.30" or "14-1530" or "930-10"
             && ! TryParseStartTimePlusEndTime(timeEntryStr, out timeEntry)
             // supported formats: "45m" or "1h30m"
             && ! TryParseDuration(timeEntryStr, out timeEntry))
            return false;
        timeEntry = timeEntry with { DayOfWeek = dayOfWeek };

        if (timeEntry.Duration > TimeSpan.FromHours(10)) throw new Error("timeEntry '{TimeEntry}' issue: to help preventing input mistakes, a time interval is not allowed not exceed 10 hours", new { TimeEntry = timeEntryStr });
        if (timeEntry.StartTime.HasValue && timeEntry.StartTime.Value.ToTimeSpan() + timeEntry.Duration >= TimeSpan.FromHours(24)) throw new Error("timeEntry '{TimeEntry}' issue: to help preventing input mistakes, start and end must be on the same day", new { TimeEntry = timeEntryStr });
        if (timeEntry.Duration.TotalMinutes < 0) throw new Error("timeEntry '{TimeEntry}' issue: end time should be after start time", new { TimeEntry = timeEntryStr });
        return true;
    }

    private static bool TryParseDayPrefix(string input, [NotNullWhen(true)] out DayOfWeek? dayOfWeek)
    {
        var match = Regex.Match(input, @"^(?<Day>[a-z]+):\d");
        if ( ! match.Success) { dayOfWeek = null; return false; }
        var dayPrefix = match.Groups["Day"].Value;

        // As a fallback: also try English weekdays.
        foreach (var culture in new[] { CultureInfo.CurrentUICulture, CultureInfo.GetCultureInfo("en") })
        {
            var found = culture.DateTimeFormat.DayNames.ToList().FindIndex(name => name.StartsWith(dayPrefix, StringComparison.InvariantCultureIgnoreCase));
            if (found < 0) continue;

            dayOfWeek = (DayOfWeek)found;
            return true;
        }

        throw new Error("detected weekdayprefix but could not parse: use 'mo:', 'tu:', 'we:', 'th:' or 'fr:'");
    }

    private static bool TryParseStartTimePlusMinutes(string input, [NotNullWhen(true)] out TimeEntry? timeEntry)
    {
        var match = Regex.Match(input, @"^(?<StartHour>\d{1,2})(?:[.:]?(?<StartMinute>\d{2}))?\+(?<TotalMinutes>\d{1,3})$");
        if (!match.Success) { timeEntry = null; return false; }

        var start = ParseTime(match.Groups["StartHour"].Value, match.Groups["StartMinute"].Value);
        var duration = TimeSpan.FromMinutes(int.Parse(match.Groups["TotalMinutes"].Value));
        timeEntry = new TimeEntry { StartTime = start, Duration = duration };
        return true;
    }

    private static bool TryParseStartTimePlusEndTime(string input, [NotNullWhen(true)] out TimeEntry? timeEntry)
    {
        var match = Regex.Match(input, @"^(?<StartHour>\d{1,2})(?:[.:]?(?<StartMinute>\d{2})?)-(?<EndHour>\d{1,2})(?:[.:]?(?<EndMinute>\d{2})?)$");
        if (!match.Success) { timeEntry = null; return false; }

        var start = ParseTime(match.Groups["StartHour"].Value, match.Groups["StartMinute"].Value);
        var end = ParseTime(match.Groups["EndHour"].Value, match.Groups["EndMinute"].Value);
        var duration = end - start;
        timeEntry = new TimeEntry { StartTime = start, Duration = duration };

        return true;
    }

    private static bool TryParseDuration(string input, [NotNullWhen(true)] out TimeEntry? timeEntry)
    {
        var match = Regex.Match(input, @"^(?:(?<Hours>\d{1,2})h)?(?:(?<Minutes>\d{1,3})m)?$");
        if (!match.Success || string.IsNullOrEmpty(match.Groups["Hours"].Value) && string.IsNullOrEmpty(match.Groups["Minutes"].Value)) { timeEntry = null; return false; }

        var minutes = 0;
        if (!string.IsNullOrEmpty(match.Groups["Minutes"].Value)) minutes = int.Parse(match.Groups["Minutes"].Value);
        if (!string.IsNullOrEmpty(match.Groups["Hours"].Value)) minutes += int.Parse(match.Groups["Hours"].Value) * 60;

        timeEntry = new TimeEntry { Duration = TimeSpan.FromMinutes(minutes) };
        return true;
    }

    private static TimeOnly ParseTime(string strHour, string strMinute)
    {
        var hour = int.Parse(strHour, CultureInfo.InvariantCulture);
        if (hour is < 0 or > 23) throw new Error("hour '{Hour}' is invalid", new { Hour = strHour });

        var minute = string.IsNullOrWhiteSpace(strMinute) ? 0 : int.Parse(strMinute, CultureInfo.InvariantCulture);
        if (minute is < 0 or > 59) throw new Error("minute '{Minute}' is invalid", new { Minute = strMinute });

        return new TimeOnly(hour, minute);
    }

    public string ToCanonicalString()
    {
        var timeString = StartTime.HasValue
            ? $"{SerializeTime(StartTime.Value)}-{SerializeTime(StartTime.Value.Add(Duration))}"
            : ToCanonicalDuration(Duration.TotalHours);

        return DayOfWeek.HasValue ? $"{SerializeDayOfWeek(DayOfWeek.Value)}:{timeString}" : timeString;

        string SerializeTime(TimeOnly time) => time.Minute == 0 ? time.Hour.ToString() : time.ToString("Hmm");
        string SerializeDayOfWeek(DayOfWeek day) => CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(day)[..2].ToLower();
    }

    public static string ToCanonicalDuration(double hours)
    {
        var totalMinutes = (int)Math.Round(hours * 60);
        var wholeHours = (int)hours;
        var remainingMinutes = totalMinutes % 60;
        if (wholeHours == 0) return $"{remainingMinutes}m";
        if (remainingMinutes == 0) return $"{wholeHours}h";
        return (totalMinutes - 60 * wholeHours) switch
        {
            0  => $"{wholeHours}h",
            15 => $"{wholeHours}h15m",
            30 => $"{wholeHours}h30m",
            45 => $"{wholeHours}h45m",
            _  => $"{wholeHours}h{remainingMinutes}m",
        };
    }

    public override string ToString() => ToCanonicalString();
}
