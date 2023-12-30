using System.Globalization;
using System.Text.RegularExpressions;

namespace Chimp;

public class TimeInterval
{
    private DateTime _baseDate;

    public bool InputContainsWeekDay { get; private set; }
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }


    public TimeInterval(string intervalString, DateTime baseDate, CultureInfo culture)
    {
        if (baseDate.Kind == DateTimeKind.Unspecified) throw new ApplicationException($"{nameof(baseDate)} cannot have DateTimeKind.Unspecified");

        _baseDate = baseDate;
        if (!TryProcessLocalizedDayPrefix(intervalString, culture)
            // English dayprefix fallback
            && !TryProcessLocalizedDayPrefix(intervalString, CultureInfo.GetCultureInfo("en")))
            throw new PebcakException("could not parse the weekdayprefix: use 'mo:', 'tu:', 'we:', 'th:' or 'fr:'");
        
        if (InputContainsWeekDay) intervalString = intervalString[3..];

        if (!TryParseStartTimePlusMinutes(intervalString) && !ParseStartTimePlusEndTime(intervalString))
            throw new PebcakException("cannot parse timeSpec '{TimeSpec}'", new() {{"TimeSpec", intervalString}});
    }

    /// <returns>false if dayprefix was invalid</returns>
    private bool TryProcessLocalizedDayPrefix(string input, CultureInfo culture)
    {
        var dayPrefixMatch = Regex.Match(input, @"^(?<Day>[a-z]{2}):");
        InputContainsWeekDay = dayPrefixMatch.Success;
        if (!InputContainsWeekDay) return true;

        var newBaseDate = Util.GetFirstDayOfWeek(_baseDate);
        for (var i = 0; i < 7; i++)
        {
            // FIXME we assume here that the first two letters of the weekday are unique for the users' language
            if (string.Equals(dayPrefixMatch.Groups["Day"].Value, string.Create(culture, $"{newBaseDate:dddd}")[..2], StringComparison.OrdinalIgnoreCase))
            {
                _baseDate = newBaseDate;
                return true;
            }
            newBaseDate = newBaseDate.AddDays(1);
        }
        
        return false;
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
        if (totalMinutes > 10 * 60) throw new PebcakException("timeSpec '{TimeSpec}' issue: to help preventing input mistakes, a time interval is not allowed not exceed 10 hours", new(){{"TimeSpec", interval}});
        End = Start.AddMinutes(totalMinutes);
        if (End.Date != Start.Date) throw new PebcakException("timeSpec '{TimeSpec}' issue: start and end must be on the same day", new(){{"TimeSpec", interval}});

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

        if (End <= Start) throw new PebcakException("timeSpec '{TimeSpec}' issue: end time should be after start time");
        if (End.Date != Start.Date) throw new PebcakException("timeSpec '{TimeSpec}' issue: start and end must be on the same day", new(){{"TimeSpec", interval}});

        return true;
    }

    private static int ParseHour(string strHour)
    {
        var hour = int.Parse(strHour);
        return hour is < 0 or > 23 ? throw new PebcakException("hour '{Hour}' is invalid", new(){{"Hour", strHour}}) : hour;
    }

    private static int ParseMinute(string strMinute)
    {
        if (string.IsNullOrWhiteSpace(strMinute)) return 0;
        var minute = int.Parse(strMinute);
        return minute is < 0 or > 59 ? throw new PebcakException("minute '{Minute}' is invalid", new(){{"Minute", strMinute}}) : minute;
    }
}