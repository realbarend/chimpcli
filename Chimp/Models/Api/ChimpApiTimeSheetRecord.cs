namespace Chimp.Models.Api;

[Serializable]
public record ChimpApiTimeSheetRecord(
    long Id,
    long CustomerId, string CustomerName, bool CustomerUnspecified,
    long ProjectId, string ProjectName, string ProjectCode, bool ProjectUnspecified,
    long TaskId, string TaskName, string TaskCode, bool TaskUnspecified, string TaskSalaryCode, long ProjectTaskId,
    long ProjectUserId, long UserId, string UserDisplayName, long? UserConnectorEmployeeId, List<string> UserTagNames,
    DateTime Date, DateTime? Start, DateTime? End,
    double Hours, double? Pause,
    string StartEnd,
    string StopTime,
    long Status,
    string Notes,

    string Timer,
    long TimeRowId,
    bool Billable,
    List<long> TagIds,
    List<string> TagNames,
    List<ChimpApiTag>? Tags,

    string ExternalUrl,
    string ExternalName,
    long StatusIntern,
    long StatusExtern,
    long? TaskBalanceTimeOffTypeId,
    string TaskBalanceTimeOffTypeName,
    long? TaskBalanceTimeOffTypeCategory,
    string ClockinPhoto,
    string ClockoutPhoto,
    string ClockinLocation,
    string ClockoutLocation,
    bool TaskBalanceSubtract,
    double TaskBalancePercentage,
    DateTime Modified,
    bool TaskBalanceOvertime,
    bool TaskBalanceOvertimeSubtract,
    double TaskBalanceOvertimePercentage,
    string Timezone);
