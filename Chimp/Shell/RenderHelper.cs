using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Chimp.Common;
using Chimp.DomainModels;

namespace Chimp.Shell;
using static Localization;

public static class RenderHelper
{
    /// <param name="alias">Supported formats: p1 or p1-1 or p1-1,2.</param>
    public static bool TryParseProjectAlias(string alias, [NotNullWhen(true)] out ShortId<ProjectTask>? project, [NotNullWhen(true)] out ShortId<Tag>[]? tags)
    {
        project = null;
        tags = null;

        var match = Regex.Match(alias, @"^p(\d{1,2})(?:-(\d{1,2}(?:,\d{1,2})*))?$");
        if (!match.Success) return false;

        project = new ShortId<ProjectTask>(int.Parse(match.Groups[1].Value));
        tags = match.Groups[2].Success ? match.Groups[2].Value.Split(',').Select(t => new ShortId<Tag>(int.Parse(t))).ToArray() : [];

        return true;
    }

    public static void RenderTimeSheetDay(TimeSheet timeSheet, DateOnly date)
    {
        var displayDay = date.IsToday() ? Localize("TODAY") : timeSheet.Date.IsCurrentWeek() ? $"{date:dddd}" : $"{date:D}";

        Console.WriteLine();
        Console.WriteLine($"{displayDay.ToUpperInvariant()} =============================================================================");

        var dayRows = timeSheet.Rows.Where(r => r.TimeDetails.Date == date).ToList();
        RenderTimeSheetDayRows(timeSheet, dayRows);

        Console.WriteLine("{0,-60} {1}", "--", "--");
        Console.WriteLine("{0,-60} {1}",
            Localize("CURRENT WEEK {TotalHours} hours of which {BillableTotal} billable",
                new { TotalHours = TimeEntry.ToCanonicalDuration(timeSheet.WeekTotal), BillableTotal = TimeEntry.ToCanonicalDuration(timeSheet.BillableTotal) }),
            $"{displayDay} {TimeEntry.ToCanonicalDuration(dayRows.Sum(r => r.TimeDetails.Hours))} {Localize("hours")}");
    }

    private static void RenderTimeSheetDayRows(TimeSheet timeSheet, List<TimeSheetRow> dayRows)
    {
        DateTime? previousEnd = null;
        foreach (var row in dayRows)
        {
            if (previousEnd != null && previousEnd < row.TimeDetails.Start) Console.WriteLine("--gap--");
            RenderTimeSheetRow(timeSheet, row);
            previousEnd = row.TimeDetails.End;
        }
    }

    public static void RenderTimeSheetRow(TimeSheet timeSheet, TimeSheetRow row)
    {
        // null values happen when a row contains projects or tags that are not accessible to the current user
        var projectShortId = timeSheet.ProjectTasks.SingleOrDefault(p => p.Id == row.ProjectTaskId)?.ShortId.ToString() ?? "??";
        var tagShortIds = row.TagIds.Select(id => timeSheet.Tags.SingleOrDefault(t => t.Id == id)?.ShortId.ToString() ?? "??").ToArray();
        var projectAlias = tagShortIds.Length == 0 ? $"p{projectShortId}" : $"p{projectShortId}-{string.Join(",", tagShortIds)}";
        var leftColumn = $"[{row.ShortId,2}] {row.TaskName ?? "???"} ({row.ProjectName ?? "???"}) {projectAlias}";

        var hasTimeOverlap = timeSheet.Rows.Any(r =>
            r.Id != row.Id && r.TimeDetails.Date == row.TimeDetails.Date &&
            r.TimeDetails.End > row.TimeDetails.Start && r.TimeDetails.Start < row.TimeDetails.End);
        var alert = hasTimeOverlap || string.IsNullOrWhiteSpace(row.Notes) || row.TimeDetails.Hours <= 0;
        var startEnd = row.TimeDetails.Start.HasValue ? $"{row.TimeDetails.Start?.ToLocalTime():HH:mm}-{row.TimeDetails.End?.ToLocalTime():HH:mm}" : new string(' ', 11);
        var tags = row.TagNames.Length > 0 ? $" [tags: {string.Join(" ", row.TagNames)}]" : "";

        // example result: [15] Task Name (Project Name) p1-2       08:00-09:15  1h15m [tags: tag1, tag2]
        Console.WriteLine($"{leftColumn,-60} {(alert ? "\b\b\u26a0 " : "")}{startEnd} {TimeEntry.ToCanonicalDuration(row.TimeDetails.Hours),5}  {row.Notes}{tags}");
    }

    public static void RenderTimeTravelerAlert(DateOnly userTime)
    {
        var objectiveTime = DateExtensions.CurrentWeek;
        var timeTravelingWeeks = Convert.ToInt32((userTime.DayNumber - objectiveTime.DayNumber) / 7);

        Console.WriteLine();
        WriteLocalized("** TIME TRAVELER ALERT  ---  you are currently {WeekCount} {Weeks} {AheadOrBehind} normal time **", new
        {
            WeekCount = Math.Abs(timeTravelingWeeks),
            Weeks = Localize(Math.Abs(timeTravelingWeeks) == 1 ? "week" : "weeks"),
            AheadOrBehind = Localize(timeTravelingWeeks < 0 ? "behind" : "ahead of").ToUpperInvariant(),
        });
    }
}
