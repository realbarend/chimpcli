using Chimp.Models.Api;

namespace Chimp.Models;

[Serializable]
public record TimeSheetRowViewModel(
    int Line,
    DateTime Date,
    DateTime? Start,
    DateTime? End,
    double Hours,
    string ProjectName,
    string ProjectSpec,
    string Tags,
    string Notes,
    ChimpApiTimeSheetRecord ApiTimeSheetRecord)
{
    public static TimeSheetRowViewModel FromApiModel(
        ChimpApiTimeSheetRecord apiTimeSheetRecord,
        int line,
        List<ProjectViewModel> projects,
        List<TagViewModel> tags)
    {
        return new TimeSheetRowViewModel(
            Line: line,
            Date: DateTime.SpecifyKind(apiTimeSheetRecord.Date, DateTimeKind.Local),
            Start: apiTimeSheetRecord.Start,
            End: apiTimeSheetRecord.End,
            Hours: apiTimeSheetRecord.Hours,
            ProjectName: $"{apiTimeSheetRecord.TaskName} ({apiTimeSheetRecord.ProjectName})",
            ProjectSpec: GetProjectSpec(apiTimeSheetRecord, projects, tags),
            Tags: string.Join(" ", apiTimeSheetRecord.Tags?.Select(t => t.Name.Truncate(20)) ?? []),
            Notes: apiTimeSheetRecord.Notes,
            ApiTimeSheetRecord: apiTimeSheetRecord);
    }

    private static string GetProjectSpec(ChimpApiTimeSheetRecord apiTimeSheetRecord, List<ProjectViewModel> projects, List<TagViewModel> tags)
    {
        var mappedProject = projects.SingleOrDefault(p => p.ApiProjectTask.Id == apiTimeSheetRecord.ProjectTaskId);
        var projectSpec = "p" + (mappedProject != null ? mappedProject.Line.ToString() : "?");
        if (apiTimeSheetRecord.Tags?.Count > 0) projectSpec += "-" + string.Join(",", apiTimeSheetRecord.Tags.Select(apiTag =>
        {
            var mappedTag = tags.SingleOrDefault(t => t.ApiTag.Id == apiTag.Id);
            return mappedTag != null ? mappedTag.Line.ToString() : "?";
        }));
        return projectSpec;
    }
}
