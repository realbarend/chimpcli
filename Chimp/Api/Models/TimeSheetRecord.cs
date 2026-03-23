namespace Chimp.Api.Models;

[Serializable]
public record TimeSheetRecord
{
    public long Id { get; init; }
    public long CustomerId { get; init; }
    public required string CustomerName { get; init; }
    public bool CustomerUnspecified { get; init; }
    public long ProjectId { get; init; }
    public required string ProjectName { get; init; }
    public string? ProjectCode { get; init; }
    public bool ProjectUnspecified { get; init; }
    public long TaskId { get; init; }
    public required string TaskName { get; init; }
    public string? TaskCode { get; init; }
    public string? TaskLedgerCode { get; init; }
    public bool TaskUnspecified { get; init; }
    public string? TaskSalaryCode { get; init; }
    public long ProjectTaskId { get; init; }
    public long ProjectUserId { get; init; }
    public long UserId { get; init; }
    public required string UserDisplayName { get; init; }
    public long? UserConnectorEmployeeId { get; init; }
    public required string[] UserTagNames { get; init; }
    public DateTime Date { get; init; }
    public DateTime? Start { get; init; }
    public DateTime? End { get; init; }
    public double Hours { get; init; }
    public double? Pause { get; init; }
    public string? StartEnd { get; init; }
    public string? StopTime { get; init; }
    public long Status { get; init; }
    public long ViewStatus { get; init; }
    public string? Notes { get; init; }
    public string? Timer { get; init; }
    public long TimeRowId { get; init; }
    public bool Billable { get; init; }
    public long[]? TagIds { get; init; }
    public string[]? TagNames { get; init; }
    public TagRecord[]? Tags { get; init; }
    public string? ExternalUrl { get; init; }
    public string? ExternalName { get; init; }
    public long StatusIntern { get; init; }
    public long StatusExtern { get; init; }
    public long? TaskBalanceTimeOffTypeId { get; init; }
    public string? TaskBalanceTimeOffTypeName { get; init; }
    public long? TaskBalanceTimeOffTypeCategory { get; init; }
    public string? ClockinPhoto { get; init; }
    public string? ClockoutPhoto { get; init; }
    public string? ClockinLocation { get; init; }
    public string? ClockoutLocation { get; init; }
    public bool TaskBalanceSubtract { get; init; }
    public double TaskBalancePercentage { get; init; }
    public DateTime Modified { get; init; }
    public bool TaskBalanceOvertime { get; init; }
    public bool TaskBalanceOvertimeSubtract { get; init; }
    public double TaskBalanceOvertimePercentage { get; init; }
    public string? Timezone { get; init; }
}
