using Chimp.Common;

namespace Chimp.DomainModels;

public record TimeSheet(TimeSheetRow[] Rows, ProjectTask[] ProjectTasks, Tag[] Tags, DateOnly Date)
{
    public double BillableTotal => Rows.Where(r => r.Billable).Sum(r => r.TimeDetails.Hours);
    public double WeekTotal => Rows.Sum(r => r.TimeDetails.Hours);

    public TimeSheetRow GetRow(ShortId<TimeSheetRow> shortId)
        => Rows.SingleOrDefault(r => r.ShortId == shortId) ?? throw new Error("previously fetched timesheet does not contain line #{Line}", new { Line = shortId });

    public ProjectTask GetProjectTask(ShortId<ProjectTask> shortId)
        => ProjectTasks.SingleOrDefault(p => p.ShortId == shortId) ?? throw new Error("previously fetched projects list does not contain line #{Line}", new { Line = shortId });

    public Tag GetTag(ShortId<Tag> shortId)
        => Tags.SingleOrDefault(t => t.ShortId == shortId) ?? throw new Error("previously fetched tags list does not contain tag {Tag}", new { Tag = "#" + shortId });
}
