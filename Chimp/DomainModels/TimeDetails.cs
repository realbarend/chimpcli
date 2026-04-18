using Chimp.Common;

namespace Chimp.DomainModels;

public record TimeDetails
{
    public DateOnly Date { get; init; }

    public DateTime? Start { get; init; }

    public DateTime? End { get; init; }

    public double Hours { get; init; }

    public static TimeDetails FromTimeEntry(TimeEntry timeEntry, TimeSheet timeSheet)
    {
        var date = CalculateDate(timeSheet, timeEntry);
        var start = !timeEntry.StartTime.HasValue ? (DateTime?)null : DateTime.SpecifyKind(date.ToDateTime(timeEntry.StartTime.Value), DateTimeKind.Local);
        return new TimeDetails
        {
            Date = date,
            Start = start,
            End = start + timeEntry.Duration,
            Hours = timeEntry.Duration.TotalHours,
        };
    }

    private static DateOnly CalculateDate(TimeSheet timeSheet, TimeEntry timeEntry)
    {
        if (timeEntry.DayOfWeek != null) return timeSheet.Date.WithWeekDay(timeEntry.DayOfWeek.Value);
        if (timeSheet.Date.IsCurrentWeek()) return DateOnly.FromDateTime(DateTime.Now);
        throw new Error("when time traveling, the timeEntry must always include the weekday, eg 'fr:{TimeEntry}'", timeEntry.ToCanonicalString());
    }

    public void Validate()
    {
        if (Hours < 0) throw new Exception("BUG: Hours cannot be negative");

        if (Start is null && End is null) return;

        if (Start is null) throw new Exception("BUG: Start must be set if End is set");
        if (End is null) throw new Exception("BUG: End must be set if Start is set");

        if (Start > End) throw new Exception("BUG: End cannot be before Start");

        if (Start.Value.Kind != DateTimeKind.Local) throw new Exception("BUG: Start must be local time");
        if (End.Value.Kind != DateTimeKind.Local) throw new Exception("BUG: End must be local time");

        // Depending on the local time of the user, Start may be at most one day away from Date.
        if (Math.Abs((Start.Value.Date - Date.ToDateTime(TimeOnly.MinValue)).Days) > 1) throw new Exception($"BUG: Start can be at most one day away from Date (Date={Date}, Start={Start})");

        // As a general protection, Date may be at most one year away from Today.
        if (Math.Abs((Date.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days) > 365) throw new Exception($"BUG: Date can be at most one year away from Today (Date={Date}, Today={DateTime.Today})");

        // Note that if both Hours and Start+End are specified, they do not need to match.
        // A special case is when Hours is 0, in which case it will be calculated automatically _serverside_ as End-Start.

        // Note that it is allowed to leave both Hours and Start+End empty.
    }
}
