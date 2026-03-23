namespace Chimp.Api.Models;

[Serializable]
public record NewTimeSheetRecord
{
    public required long CustomerId { get; init; }
    public required long ProjectId { get; init; }
    public required long ProjectTaskId { get; init; }
    public required DateTime Date { get; init; }
    public required double Hours { get; init; }
    public required long[]? TagIds { get; init; }
    public required string? Notes { get; init; }
    public required DateTime? Start { get; init; }
    public required DateTime? End { get; init; }
    public required string? StartEnd { get; init; }
    public required string? Timezone { get; init; }
}
