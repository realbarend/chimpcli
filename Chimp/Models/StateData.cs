using Chimp.Models.Api;

namespace Chimp.Models;

public class StateData
{
    public ChimpApiUser? User { get; set; }
    public string? AuthToken { get; set; }
    public string? LoginUserName { get; set; }
    public string? LoginPassword { get; set; }
    public DateTimeOffset? AuthTokenExpirationDate { get; set; }
    public DateTime? TimeTravelingDate { get; set; }
    public List<TimeSheetViewRow>? CachedTimeSheet { get; set; }
    public List<ProjectViewRow>? CachedProjects { get; set; }
}