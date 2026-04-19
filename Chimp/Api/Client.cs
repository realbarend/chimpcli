using System.Net.Http.Headers;
using System.Text.Json;
using Chimp.Api.Models;
using Chimp.Common;
using Chimp.DomainModels;

namespace Chimp.Api;

public class Client(PersistablePropertyBag stateBag, ICognitoAuthentication authentication, HttpClient httpClient, DebugLogger logger) : IClient
{
    private readonly TimeSheetCache _timeSheetCache = new();

    public async Task Login(string userName, string password)
    {
        try
        {
            await authentication.Login(userName, password);
        }
        catch (Exception e)
        {
            throw new Error("login attempt failed: {Message}. Note: timechimp may temporarily block your account after multiple failures.", e.Message);
        }

        // we read and cache the user at login, so it is fetched exactly one time from the api
        stateBag.Set((await ApiCall(HttpMethod.Get, "user/current")).Deserialize<UserRecord>(false));
        _timeSheetCache.Clear();
        ClearProjectsCache();
    }

    public void ClearProjectsCache()
    {
        stateBag.Delete<ProjectTask[]>();
        stateBag.Delete<Tag[]>();
    }

    public User GetUser() => stateBag.Get<UserRecord>()?.ToUser() ?? throw new ApplicationException("not logged in");

    /// <summary>ProjectTasks are cached until explicity flushed.</summary>
    public async Task<ProjectTask[]> GetProjectTasks()
    {
        var cached = stateBag.Get<ProjectTask[]>();
        if (cached != null) return cached;

        var projects = new List<ProjectTask>();
        var shortId = 0;
        foreach (var project in (await ApiCall(HttpMethod.Get, $"project/{GetUser().Id}/uiselectbyuser")).Deserialize<List<ProjectRecord>>(false).OrderBy(p => p.Intern).ThenBy(p => p.Name))
        foreach (var projectTask in (await ApiCall(HttpMethod.Get, $"projecttask/uiselect%2Fproject/{project.Id}")).Deserialize<List<ProjectTaskRecord>>(false).OrderBy(t => t.Name))
            projects.Add(new ProjectTask { ShortId = new ShortId<ProjectTask>(++shortId), Id = projectTask.Id, ProjectId = project.Id, CustomerId = project.CustomerId, ProjectName = project.Name, TaskName = projectTask.Name });
        var arr = projects.ToArray();
        stateBag.Set(arr);
        return arr;
    }

    /// <summary>Tags are cached until explicity flushed.</summary>
    public async Task<Tag[]> GetTags()
    {
        var cached = stateBag.Get<Tag[]>();
        if (cached != null) return cached;

        var tags = (await ApiCall(HttpMethod.Get, "tag/type%2F1")).Deserialize<TagRecord[]>()
            .OrderBy(t => t.Name).Select((t, i) => new Tag { ShortId = new ShortId<Tag>(i + 1), Id = t.Id, Name = t.Name }).ToArray();
        stateBag.Set(tags);
        return tags;
    }

    /// <summary>To reduce api calls, timesheet data is cached during the lifetime of the client. Changes are tracked in memory.</summary>
    public async Task<TimeSheetRow[]> GetTimeSheetRows(DateOnly date) => TimeSheetRecordMapper.MapToTimeSheetRows(await EnsureCachedTimeSheetRecords(date));

    public async Task<TimeSheetRow> AddTimeSheetRow(TimeSheetRowDto dto)
    {
        var obj = TimeSheetRecordMapper.MapToNewRecord(dto, (await GetProjectTasks()).SingleOrDefault(p => p.Id == dto.ProjectTaskId));
        logger.Log("[DEBUG] about to add using dto: " + JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true }));
        logger.Log("[DEBUG] about to add using obj: " + JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
        var newRecord = (await ApiCall(HttpMethod.Post, "time", obj)).Deserialize<TimeSheetRecord>();
        logger.Log("[DEBUG] added record: " + JsonSerializer.Serialize(newRecord, new JsonSerializerOptions { WriteIndented = true }));

        // Update the cache. If we don't have a cache yet, don't bother.
        _timeSheetCache.GetRecords(dto.TimeDetails.Date)?.Add(newRecord.Id, newRecord);

        return (await GetTimeSheetRows(dto.TimeDetails.Date)).Single(r => r.Id == newRecord.Id);
    }

    public async Task<TimeSheetRow> UpdateTimeSheetRow(long id, TimeSheetRowDto dto)
    {
        // Need the cached record, because the api doesn't allow partial updates.
        var cachedRecords = await EnsureCachedTimeSheetRecords(dto.TimeDetails.Date);
        var record = cachedRecords[id];

        logger.Log("[DEBUG] about to update with dto: " + JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true }));
        logger.Log("[DEBUG] original record: " + JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));

        record = TimeSheetRecordMapper.MapToUpdateRecord(record, dto, (await GetProjectTasks()).SingleOrDefault(p => p.Id == dto.ProjectTaskId));

        logger.Log("[DEBUG] updating record: " + JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));

        var updatedRecord = (await ApiCall(HttpMethod.Put, $"time/{record.Id}", record)).Deserialize<TimeSheetRecord>();
        cachedRecords[record.Id] = updatedRecord;

        logger.Log("[DEBUG] updated record: " + JsonSerializer.Serialize(updatedRecord, new JsonSerializerOptions { WriteIndented = true }));

        return TimeSheetRecordMapper.MapToTimeSheetRows(cachedRecords).Single(r => r.Id == updatedRecord.Id);
    }

    public async Task DeleteTimeSheetRow(TimeSheetRow row)
    {
        await ApiCall(HttpMethod.Delete, $"time/{row.Id}", Array.Empty<int>());

        // Update the cache. If we don't have a cache yet, don't bother.
        _timeSheetCache.GetRecords(row.TimeDetails.Date)?.Remove(row.Id);
    }

    private async Task<Dictionary<long, TimeSheetRecord>> EnsureCachedTimeSheetRecords(DateOnly date)
    {
        var cachedRecords = await _timeSheetCache.GetOrUpdateRecords(date.WeekStart,
            async () => (await ApiCall(HttpMethod.Get, $"time/week/{GetUser().Id}/{date.WeekStart:yyyy-MM-dd}")).Deserialize<List<TimeSheetRecord>>().ToDictionary(r => r.Id, r => r));
        return cachedRecords;
    }

    private async Task<string> ApiCall(HttpMethod method, string path, object? data = null)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("Accept", "application/json");
        var bearer = await authentication.GetValidBearerToken() ?? throw new Error("attempted to call api with no authtoken (did you log in?)");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        if (data != null) request.Content = new StringContent(data.Serialize(), new MediaTypeHeaderValue("application/json"));

        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new Error("api returned httpcode {Code} ({CodeString}): if this persists, try to login", (int)response.StatusCode, response.StatusCode.ToString());
        var bodyString = await response.Content.ReadAsStringAsync();
        if (bodyString.TrimStart().StartsWith('<')) throw new Error("api returned html: probably need to re-authorize");

        return bodyString;
    }
}
