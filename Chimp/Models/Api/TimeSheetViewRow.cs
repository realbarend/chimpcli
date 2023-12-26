namespace Chimp.Models.Api;

public record TimeSheetViewRow(int Line, DateTime Date, DateTime? Start, DateTime? End, double Hours, string ProjectName, int? ProjectLine, string TaskName, string Notes, ChimpApiTimeSheetRecord ChimpApiTimeSheetRecord);
