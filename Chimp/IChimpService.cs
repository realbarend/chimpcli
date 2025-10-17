using Chimp.Models;

namespace Chimp;

public interface IChimpService
{
    Task<bool> DoLoginIfPersistedCredentials();
    Task DoLogin(string userName, string password, bool persistCredentials);
    Localizer GetLocalizer();
    Task<List<ProjectViewModel>> GetProjects();
    Task<List<TagViewModel>> GetTags();
    Task<(double WeekTotal, double BillableTotal, List<TimeSheetRowViewModel> TimeSheet)> GetTimeSheet(int? weekOffset);
    TimeSheetRowViewModel GetCachedTimeSheetViewRow(int line);
    DateTime? GetTimeTravelingDate();
    Task AddRow(int projectLine, IEnumerable<int> tagLines, TimeInterval interval, string notes);
    Task UpdateNotes(int line, string notes);
    Task UpdateTimeInterval(int line, TimeInterval interval);
    Task UpdateProject(int line, int projectLine, IEnumerable<int> tagLines);
    Task DeleteRow(int line);
}
