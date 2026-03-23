namespace Chimp.DomainModels;

public class TimeSheetRowDto
{
    public required long ProjectTaskId { get; set; }

    public required TimeDetails TimeDetails { get; set; }

    public long[] TagIds { get; set; } = [];
    public string? Notes { get; set; }

    public static TimeSheetRowDto FromExistingRow(TimeSheetRow row)
    {
        return new TimeSheetRowDto
        {
            ProjectTaskId = row.ProjectTaskId,
            TimeDetails = row.TimeDetails,
            TagIds = row.TagIds,
            Notes = row.Notes,
        };
    }
}
