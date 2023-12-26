using JetBrains.Annotations;

namespace Chimp.Models.Api;

[UsedImplicitly]
public record ChimpApiTimeSheetRecord(
    long Id,
    long CustomerId, string CustomerName,
    long ProjectId, string ProjectName, string ProjectCode, bool ProjectUnspecified,
    long TaskId, string TaskName, string TaskCode, string TaskSalaryCode, long ProjectTaskId,
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
    List<ChimpApiTimeSheetRecordTag> Tags,

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
    
[UsedImplicitly]
public record ChimpApiTimeSheetRecordTag(long CompanyId, string Name, bool Active, long Type, long Id);