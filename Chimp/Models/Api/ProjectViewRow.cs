namespace Chimp.Models.Api;

public record ProjectViewRow(int Line, long CustomerId, long ProjectId, long ProjectTaskId, string? ProjectName, string? TaskName);
