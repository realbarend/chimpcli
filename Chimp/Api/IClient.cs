using Chimp.DomainModels;

namespace Chimp.Api;

public interface IClient
{
    Task Login(string userName, string password);
    void ClearProjectsCache();
    User GetUser();

    /// <summary>ProjectTasks are cached until explicity flushed.</summary>
    Task<ProjectTask[]> GetProjectTasks();

    /// <summary>Tags are cached until explicity flushed.</summary>
    Task<Tag[]> GetTags();

    /// <summary>To reduce api calls, timesheet data is cached during the lifetime of the client. Changes are tracked in memory.</summary>
    Task<TimeSheetRow[]> GetTimeSheetRows(DateOnly date);

    Task<TimeSheetRow> AddTimeSheetRow(TimeSheetRowDto dto);
    Task<TimeSheetRow> UpdateTimeSheetRow(long id, TimeSheetRowDto dto);
    Task DeleteTimeSheetRow(TimeSheetRow row);
}
