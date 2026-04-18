using Chimp.Api;
using Chimp.Common;
using Chimp.DomainModels;

namespace Chimp.Services;

public class TimeSheetService(PersistablePropertyBag stateBag, IClient api, DebugLogger logger)
{
    private record ApiCredentials(string UserName, string Password);
    private record TimeSheetDate(DateOnly Date);

    public async Task Login(string userName, string password, bool persistCredentials)
    {
        await api.Login(userName, password);
        LanguageHelper.SetUiLanguage(GetUserLanguage());
        if (persistCredentials) stateBag.Set(new ApiCredentials(userName, password));
        else stateBag.Delete<ApiCredentials>();
    }

    public void ClearProjectsCache() => api.ClearProjectsCache();

    public async Task<bool> TryLoginUsingPersistedCredentials()
    {
        var credentials = stateBag.Get<ApiCredentials>();
        if (credentials == null) return false;
        logger.Log("** logging in using previously persisted credentials");
        try
        {
            await Login(credentials.UserName, credentials.Password, true);
            return true;
        }
        catch (Exception e)
        {
            logger.Log($"** login using persisted credentials failed: {e.Message}");
            return false;
        }
    }

    public string GetUserLanguage()
    {
        try
        {
            return api.GetUser().Language;
        }
        catch
        {
            // default when not logged in:
            return "en";
        }
    }

    public void SetWeekOffset(int weekOffset)
    {
        if (weekOffset is < -52 or > 52) throw new Error("invalid weekOffset: time travel is allowed for maximum 52 weeks");
        if (weekOffset == 0) stateBag.Delete<TimeSheetDate>();
        else stateBag.Set(new TimeSheetDate(DateExtensions.CurrentWeek.AddDays(7 * weekOffset)));
    }

    public async Task<TimeSheet> GetTimeSheet()
    {
        var date = stateBag.Get<TimeSheetDate>()?.Date ?? DateExtensions.CurrentWeek;

        // Make sure to remove a previously set week offset, in case the user appears to have been time traveling but now appears to have arrived in current time.
        if (date.IsCurrentWeek()) stateBag.Delete<TimeSheetDate>();

        return new TimeSheet(await api.GetTimeSheetRows(date), await api.GetProjectTasks(), await api.GetTags(), date);
    }

    public async Task<TimeSheetRow> AddTimeSheetRow(ShortId<ProjectTask> project, ShortId<Tag>[] tagIds, TimeEntry timeEntry, string? notes)
    {
        var timeSheet = await GetTimeSheet();
        var projectTask = timeSheet.GetProjectTask(project);
        var tags = tagIds.Select(timeSheet.GetTag).ToArray();

        var dto = new TimeSheetRowDto
        {
            ProjectTaskId = projectTask.Id,

            TimeDetails = TimeDetails.FromTimeEntry(timeEntry, timeSheet),

            TagIds = tags.Select(t => t.Id).ToArray(),
            Notes = notes,
        };

        return await api.AddTimeSheetRow(dto);
    }

    public async Task<TimeSheetRow> UpdateTimeSheetRow(ShortId<TimeSheetRow> rowShortId, ShortId<ProjectTask>? project, ShortId<Tag>[]? tags, TimeEntry? timeEntry, string? notes)
    {
        var timeSheet = await GetTimeSheet();
        var row = timeSheet.GetRow(rowShortId);

        var dto = TimeSheetRowDto.FromExistingRow(row);

        if (timeEntry != null)
        {
            // when updating a time, the effective weekday defaults to the existing row date
            timeEntry = timeEntry with { DayOfWeek = timeEntry.DayOfWeek ?? row.TimeDetails.Date.DayOfWeek };
            dto.TimeDetails = TimeDetails.FromTimeEntry(timeEntry, timeSheet);
        }

        if (project != null) dto.ProjectTaskId = timeSheet.GetProjectTask(project).Id;

        if (tags != null) dto.TagIds = tags.Select(shortId => timeSheet.GetTag(shortId).Id).ToArray();

        if (notes != null) dto.Notes = notes;

        return await api.UpdateTimeSheetRow(row.Id, dto);
    }

    public async Task<TimeSheetRow> CopyTimeSheetRow(ShortId<TimeSheetRow> shortId, TimeEntry timeEntry)
    {
        var timeSheet = await GetTimeSheet();
        var row = timeSheet.GetRow(shortId);

        var projectTask = timeSheet.ProjectTasks.SingleOrDefault(p => p.Id == row.ProjectTaskId)?.ShortId
                          ?? throw new Error("cannot copy row #{Line}, because the project or tag is not available", shortId);
        var tags = row.TagIds.Select(id => timeSheet.Tags.SingleOrDefault(t => t.Id == id)?.ShortId ?? throw new Error("cannot copy row #{Line}, because the project or tag is not available")).ToArray();
        timeEntry = timeEntry with { DayOfWeek = timeEntry.DayOfWeek ?? row.TimeDetails.Date.DayOfWeek };

        return await AddTimeSheetRow(projectTask, tags, timeEntry, row.Notes);
    }

    public async Task DeleteRow(TimeSheetRow row)
    {
        await api.DeleteTimeSheetRow(row);
    }
}
