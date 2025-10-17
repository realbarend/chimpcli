using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Chimp;

public record TimeInterval(DateTime Start, DateTime End)
{
    public static bool TryParse(string intervalString, DateTime baseDate, CultureInfo culture, [NotNullWhen(true)] out TimeInterval? timeInterval)
    {
        if (baseDate.Kind == DateTimeKind.Unspecified) throw new ApplicationException($"{nameof(baseDate)} cannot have DateTimeKind.Unspecified");

        // supported format: "ww:<time>" where ww is first two letters of the weekday, e.g. 'mo' for Monday
        if (TryParseDayPrefix(intervalString, culture, out var weekDay))
        {
            // strip weekday so we can process the time-part
            intervalString = intervalString[3..];
            baseDate = Util.GetFirstDayOfWeek(baseDate).AddDays(weekDay.Value);
        }

        timeInterval =
            // supported formats: "14.30+15" or "14+30" or "930+60" or "9:30+30"
            ParseStartTimePlusMinutes(intervalString, baseDate)
            // supported formats: "14.30-15.30" or "14-1530" or "930-10"
            ?? ParseStartTimePlusEndTime(intervalString, baseDate);

        return timeInterval != null;
    }

    public static TimeInterval Parse(string intervalString, DateTime baseDate, CultureInfo culture)
    {
        return TryParse(intervalString, baseDate, culture, out var timeInterval)
            ? timeInterval
            : throw new PebcakException("cannot parse timeSpec '{TimeSpec}'", new() {{"TimeSpec", intervalString}});
    }

    public static bool TryParseDayPrefix(string input, CultureInfo culture, [NotNullWhen(true)] out int? weekDayNumber)
    {
        weekDayNumber = null;
        var dayPrefixMatch = Regex.Match(input, @"^(?<Day>[a-z]{2}):\d");
        if (!dayPrefixMatch.Success) return false;
        var dayPrefix = dayPrefixMatch.Groups["Day"].Value;

        var monday = Util.GetFirstDayOfWeek(DateTime.Now);
        for (weekDayNumber = 0; weekDayNumber < 7; weekDayNumber++)
        {
            // FIXME we assume here that the first two letters of the weekday are unique for the users' language
            if (string.Equals(dayPrefix, string.Create(culture, $"{monday.AddDays(weekDayNumber.Value):dddd}")[..2], StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // let's also try English weekdays
        if (culture.Name != "en") return TryParseDayPrefix(input, CultureInfo.GetCultureInfo("en"), out weekDayNumber);

        throw new PebcakException("detected weekdayprefix but could not parse: use 'mo:', 'tu:', 'we:', 'th:' or 'fr:'");
    }

    private static TimeInterval? ParseStartTimePlusMinutes(string input, DateTime baseDate)
    {
        var match = Regex.Match(input, @"^(?<StartHour>\d{1,2})(?:[.:]?(?<StartMinute>\d{2}))?\+(?<TotalMinutes>\d{1,3})$");
        if (!match.Success) return null;

        var start = baseDate.Date.WithTime(ParseTime(match.Groups["StartHour"].Value, match.Groups["StartMinute"].Value));
        var totalMinutes = int.Parse(match.Groups["TotalMinutes"].Value);
        if (totalMinutes > 10 * 60) throw new PebcakException("timeSpec '{TimeSpec}' issue: to help preventing input mistakes, a time interval is not allowed not exceed 10 hours", new(){{"TimeSpec", input}});
        var end = start.AddMinutes(totalMinutes);
        if (end.Date != start.Date) throw new PebcakException("timeSpec '{TimeSpec}' issue: to help preventing input mistakes, start and end must be on the same day", new(){{"TimeSpec", input}});

        return new TimeInterval(start, end);
    }

    private static TimeInterval? ParseStartTimePlusEndTime(string input, DateTime baseDate)
    {
        var match = Regex.Match(input, @"^(?<StartHour>\d{1,2})(?:[.:]?(?<StartMinute>\d{2})?)-(?<EndHour>\d{1,2})(?:[.:]?(?<EndMinute>\d{2})?)$");
        if (!match.Success) return null;

        var start = baseDate.Date.WithTime(ParseTime(match.Groups["StartHour"].Value, match.Groups["StartMinute"].Value));
        var end = baseDate.Date.WithTime(ParseTime(match.Groups["EndHour"].Value, match.Groups["EndMinute"].Value));

        if (end <= start) throw new PebcakException("timeSpec '{TimeSpec}' issue: end time should be after start time");
        if (end.Date != start.Date) throw new PebcakException("timeSpec '{TimeSpec}' issue: to help preventing input mistakes, start and end must be on the same day", new(){{"TimeSpec", input}});

        return new TimeInterval(start, end);
    }

    private static TimeOnly ParseTime(string strHour, string strMinute)
    {
        var hour = int.Parse(strHour, CultureInfo.InvariantCulture);
        if (hour is < 0 or > 23) throw new PebcakException("hour '{Hour}' is invalid", new(){{"Hour", strHour}});

        var minute = string.IsNullOrWhiteSpace(strMinute) ? 0 : int.Parse(strMinute, CultureInfo.InvariantCulture);
        if (minute is < 0 or > 59) throw new PebcakException("minute '{Minute}' is invalid", new() { { "Minute", strMinute } });

        return new TimeOnly(hour, minute);
    }
}
