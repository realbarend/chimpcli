using Chimp.Models.Api;

namespace Chimp.Models;

[Serializable]
public class StateData
{
    public AuthProperties? Auth { get; set; }
    public ChimpApiUser? User { get; set; }
    public DateTime? TimeTravelingDate { get; set; }
    public List<TimeSheetRowViewModel>? CachedTimeSheet { get; set; }
    public List<ProjectViewModel>? CachedProjects { get; set; }
    public List<TagViewModel>? CachedTags { get; set; }
}
