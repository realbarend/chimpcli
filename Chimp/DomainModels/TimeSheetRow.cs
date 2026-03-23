namespace Chimp.DomainModels;

public record TimeSheetRow
{
    public required ShortId<TimeSheetRow> ShortId { get; init; }
    public required long Id { get; init; }
    public required long ProjectTaskId { get; init; }
    public required TimeDetails TimeDetails { get; init; }
    public required long[] TagIds { get; init; }
    public required string? Notes { get; init; }

    // below fields are only present for display purposes
    public required string? ProjectName { get; init; }
    public required string? TaskName { get; init; }
    public required string[] TagNames { get; init; }
    public required bool Billable { get; init; }
}
