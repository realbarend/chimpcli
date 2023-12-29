using Chimp.Models.Api;

namespace Chimp.Models;

[Serializable]
public class StateData
{
    public ChimpApiUser? User { get; set; }
    public string? AuthToken { get; set; }
    public string? LoginUserName { get; set; }
    public string? LoginPassword { get; set; }
    public DateTimeOffset? AuthTokenExpirationDate { get; set; }
    public DateTime? TimeTravelingDate { get; set; }
    public List<TimeSheetRowViewModel>? CachedTimeSheet { get; set; }
     public List<ProjectViewModel>? CachedProjects { get; set; }
     public List<TagViewModel>? CachedTags { get; set; }
 }
