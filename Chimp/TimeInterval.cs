using System.Text.RegularExpressions;

namespace Chimp;

public class TimeInterval
{
    private readonly string _rawInterval;
    private DateTime _baseDate;
    private readonly Localizer _localizer;

    public bool InputContainsWeekDay { get; private set; }
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }


    public TimeInterval(string intervalString, DateTime baseDate, Localizer localizer)
    {
        if (baseDate.Kind == DateTimeKind.Unspecified) throw new ApplicationException($"{nameof(baseDate)} cannot have DateTimeKind.Unspecified");

        _rawInterval = intervalString;
        _baseDate = baseDate;
        _localizer = localizer;

        ProcessLocalizedDayPrefix(intervalString);
        if (InputContainsWeekDay) intervalString = intervalString[3..];

        if (!TryParseStartTimePlusMinutes(intervalString) && !ParseStartTimePlusEndTime(intervalString))
            throw new PebcakException($"could not parse {nameof(intervalString)}");
    }

    private void ProcessLocalizedDayPrefix(string input)
    {
        var dayPrefixMatch = Regex.Match(input, @"^(?<Day>[a-z]{2}):");
        InputContainsWeekDay = dayPrefixMatch.Success;
        if (!InputContainsWeekDay) return;

        _baseDate = Util.GetFirstDayOfWeek(_baseDate);
        for (var i = 0; i < 7; i++)
        {
            // FIXME we assume here that the first two letters of the weekday are unique for the users' language
            if (string.Equals(dayPrefixMatch.Groups["Day"].Value, _localizer.GetWeekDay(_baseDate)[..2], StringComparison.OrdinalIgnoreCase)) return;
            _baseDate = _baseDate.AddDays(1);
        }

        throw new PebcakException("could not parse weekdayprefix: make sure to use the language known in timechimp, e.g. use 'ma:' if you are Dutch or 'mo:' if you are English");
    }

    private bool TryParseStartTimePlusMinutes(string interval)
    {
        // example matches: "14.30+15" or "14+30" or "930+60" or "9:30+30"
        var matchRelative = Regex.Match(interval, @"^(?<StartHour>\d{1,2})(?:[.:]?(?<StartMinute>\d{2}))?\+(?<TotalMinutes>\d+)$");
        if (!matchRelative.Success) return false;

        Start = _baseDate.Date
            .AddHours(ParseHour(matchRelative.Groups["StartHour"].Value))
            .AddMinutes(ParseMinute(matchRelative.Groups["StartMinute"].Value));

        var totalMinutes = int.Parse(matchRelative.Groups["TotalMinutes"].Value);
        if (totalMinutes > 10 * 60) throw new PebcakException("to help preventing input mistakes, a time interval is not allowed not exceed 10 hours");
        End = Start.AddMinutes(totalMinutes);
        if (End.Date != Start.Date) throw new PebcakException($"{nameof(interval)} start and end must be on the same day");

        return true;
    }

    private bool ParseStartTimePlusEndTime(string interval)
    {
        // example matches: "14.30-15.30" or "14-1530" or "930-10"
        var matchEndtime = Regex.Match(interval, @"^(?<StartHour>\d{1,2})(?:[.:]?(?<StartMinute>\d{2}))?-(?<EndHour>\d{1,2})(?:[.:]?(?<EndMinute>\d{2}))?$");
        if (!matchEndtime.Success) return false;

        Start = _baseDate.Date
            .AddHours(ParseHour(matchEndtime.Groups["StartHour"].Value))
            .AddMinutes(ParseMinute(matchEndtime.Groups["StartMinute"].Value));

        End = _baseDate.Date
            .AddHours(ParseHour(matchEndtime.Groups["EndHour"].Value))
            .AddMinutes(ParseMinute(matchEndtime.Groups["EndMinute"].Value));

        if (End <= Start) throw new PebcakException($"{nameof(interval)} end time should be after start time");
        if (End.Date != Start.Date) throw new PebcakException($"{nameof(interval)} start and end must be on the same day");

        return true;
    }

    private static int ParseHour(string strHour)
    {
        var hour = int.Parse(strHour);
        return hour is < 0 or > 23 ? throw new PebcakException("hour is out of range") : hour;
    }

    private static int ParseMinute(string strMinute)
    {
        if (string.IsNullOrWhiteSpace(strMinute)) return 0;
        var minute = int.Parse(strMinute);
        return minute is < 0 or > 59 ? throw new PebcakException("minute is out of range") : minute;
    }
}