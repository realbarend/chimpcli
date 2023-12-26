using System.Net.Http.Headers;
using Chimp.Models;
using Chimp.Models.Api;

namespace Chimp;

public class ChimpService
{
    private readonly string _stateFilePath;
    private readonly StateData _state = new();
    
    public ChimpService(string stateFilePath)
    {
        _stateFilePath = stateFilePath;
        try
        {
            if (File.Exists(_stateFilePath)) _state = ReadPersistedState();
        }
        catch (Exception e)
        {
            // don't throw here because that would also block the login flow
            Console.WriteLine($"** failed to read state-file: {e.Message} -- you may be able to fix this by logging in again");
        }
    }

    public async Task<bool> DoLoginIfPersistedCredentials()
    {
        if (_state.LoginUserName == null || _state.LoginPassword == null) return false;
        Console.WriteLine($"Logging in using previously persisted credentials");
        try
        {
            await DoLogin(_state.LoginUserName, _state.LoginPassword, true);
        }
        catch
        {
            Console.WriteLine("** removing your persisted credentials because of failed login");
            _state.LoginUserName = null;
            _state.LoginPassword = null;
            PersistState();
            throw;
        }
        return true;
    }

    public async Task DoLogin(string userName, string password, bool persistCredentials)
    {
        var uri = new Uri("https://app.timechimp.com/account/login?ReturnUrl=%2F");
        var response = await new HttpClient().PostAsync(uri, new FormUrlEncodedContent(new Dictionary<string, string>  
        {  
            { "UserName", userName}, { "Password", password}, { "RememberMe", "true"},
        }));
        if (!response.IsSuccessStatusCode) throw new ApiException($"login failed: got httpcode {response.StatusCode}");

        var cookie = response.GetCookie(uri, ".AspNet.ApplicationCookie");
        if (cookie?.Value == null) throw new PebcakException("login attempt failed, maybe wrong password. note: timechimp can temporarily block your account after multiple failures");
        Console.WriteLine($"Login successful, got authtoken valid until {cookie.Expires.ToLocalTime():D}.");

        if (persistCredentials) { _state.LoginUserName = userName; _state.LoginPassword = password; }
        _state.AuthToken = cookie.Value;
        _state.AuthTokenExpirationDate = cookie.Expires;
        _state.User = Util.JsonDeserialize<ChimpApiUser>(await ApiCall(HttpMethod.Get, "user/current"), false);
        PersistState();
    }

    public Localizer GetLocalizer()
    {
        if (_state.User == null) throw new PebcakException("this action requires you login first");
        return new Localizer(Environment.GetEnvironmentVariable("CHIMPCLI_LANGUAGE") ?? _state.User.Language);
    }

    public async Task<IList<ProjectViewRow>> GetProjects()
    {
        if (_state.User == null) throw new PebcakException("cannot list projects without first authorizing: login first");

        var projects = new List<ProjectViewRow>();
        var rawProjects = await ApiCall(HttpMethod.Get, $"project/{_state.User.UserName}/uiselectbyuser");
        foreach (var apiProject in Util.JsonDeserialize<List<ChimpApiProject>>(rawProjects, false).OrderBy(p => p.Intern).ThenBy(p => p.Name))
        {
            var rawTasks = await ApiCall(HttpMethod.Get, $"projecttask/uiselect%2Fproject/{apiProject.Id}");
            foreach (var apiTask in Util.JsonDeserialize<List<ChimpApiProjectTask>>(rawTasks, false).OrderBy(t => t.Name))
            {
                projects.Add(new ProjectViewRow(projects.Count + 1, apiProject.CustomerId, apiProject.Id, apiTask.Id, apiProject.Name, apiTask.Name));
            }
        }

        _state.CachedProjects = projects;
        PersistState();
        return _state.CachedProjects;
    }

    public async Task<(double WeekTotal, double BillableTotal, IList<TimeSheetViewRow> ViewRows)> GetTimeSheet(int? weekOffset)
    {
        if (_state.User == null) throw new PebcakException("cannot get timesheet without first authorizing: login first");
        if (_state.CachedProjects == null) await GetProjects();
        ProcessWeekOffset();

        var responseBody = await ApiCall(HttpMethod.Get, $"time/week/{_state.User.UserName}/{_state.TimeTravelingDate ?? DateTime.Now:yyyy-MM-dd}");
        var data = Util.JsonDeserialize<List<ChimpApiTimeSheetRecord>>(responseBody);
        
        _state.CachedTimeSheet = data
            .OrderBy(r => r.Date).ThenBy(r => r.Start)
            .Select((r, i) => new TimeSheetViewRow(i + 1, DateTime.SpecifyKind(r.Date, DateTimeKind.Local), r.Start, r.End, r.Hours, r.ProjectName, _state.CachedProjects?.SingleOrDefault(p => p.ProjectTaskId == r.ProjectTaskId)?.Line, r.TaskName, r.Notes, r))
            .ToList();
        PersistState();
        return (data.Sum(r => r.Hours), data.Where(d => d.Billable).Sum(r => r.Hours), _state.CachedTimeSheet);

        void ProcessWeekOffset()
        {
            if (weekOffset != null)
            {
                if (weekOffset is < -52 or > 52) throw new PebcakException("invalid week offset: time travel is allowed for maximum 52 weeks");
                _state.TimeTravelingDate = weekOffset == 0 ? null : DateTime.Now.AddDays(7 * weekOffset.Value);
            }
        
            if (_state.TimeTravelingDate != null && Util.GetFirstDayOfWeek(_state.TimeTravelingDate.Value) == Util.GetFirstDayOfWeek(DateTime.Today))
            {
                // user was previously time traveling, but appears to have arrived in current time, so we can reset
                _state.TimeTravelingDate = null;
            }
        }
    }

    public DateTime? GetTimeTravelingDate() => _state.TimeTravelingDate;

    public async Task AddRow(int projectLine, TimeInterval interval, string notes)
    {
        if (_state.User == null) throw new PebcakException("cannot track time without first authorizing: login first");
        if (_state.CachedProjects == null) throw new PebcakException("cannot set a project without first fetching the project list");
        
        var project = _state.CachedProjects.SingleOrDefault(p => p.Line == projectLine)
                      ?? throw new PebcakException($"local cache does not contain line #{projectLine}");

        await ApiCall(HttpMethod.Post, "time", new
        {
            _state.User.Id,
            project.CustomerId,
            project.ProjectId,
            project.ProjectTaskId,
            Date = interval.Start,
            Start = interval.Start,
            End = interval.End,
            Hours = (interval.End - interval.Start).TotalHours,
            StartEnd = $"{interval.Start:HH:mm}-{interval.End:HH:mm}",
            Notes = notes,
        });
    }
    
    public async Task UpdateNotes(int line, string notes)
    {
        if (_state.CachedTimeSheet == null) throw new PebcakException("you cannot update timesheet data without first fetching the timesheet");
        
        var existingRow = _state.CachedTimeSheet.SingleOrDefault(r => r.Line == line)?.ChimpApiTimeSheetRecord
                          ?? throw new PebcakException($"local cache does not contain line #{line}");
        var updatedRow = existingRow with { Notes = notes, Modified = DateTime.Now };

        await ApiCall(HttpMethod.Put, "time/put", updatedRow);
    }

    public TimeSheetViewRow GetCachedTimeSheetViewRow(int line)
    {
        if (_state.CachedTimeSheet == null) throw new PebcakException("you cannot update timesheet data without first fetching the timesheet");
        
        return _state.CachedTimeSheet.SingleOrDefault(r => r.Line == line)
                  ?? throw new PebcakException($"local cache does not contain line #{line}");
    }
    
    public async Task UpdateTimeInterval(int line, TimeInterval interval)
    {
        if (_state.CachedTimeSheet == null) throw new PebcakException("you cannot update timesheet data without first fetching the timesheet");
        
        var existingRow = _state.CachedTimeSheet.SingleOrDefault(r => r.Line == line)?.ChimpApiTimeSheetRecord
                          ?? throw new PebcakException($"local cache does not contain line #{line}");
        
        var updatedRow = existingRow with
        {
            Date = interval.Start,
            Start = interval.Start,
            End = interval.End,
            Hours = (interval.End - interval.Start).TotalHours,
            StartEnd = $"{interval.Start:HH:mm}-{interval.End:HH:mm}",
            Modified = DateTime.Now,
        };

        await ApiCall(HttpMethod.Put, "time/put", updatedRow);
    }
    
    public async Task UpdateProject(int line, int projectLine)
    {
        if (_state.CachedTimeSheet == null) throw new PebcakException("you cannot update timesheet data without first fetching the timesheet");
        if (_state.CachedProjects == null) throw new PebcakException("cannot set a project without first fetching the project list");

        var project = _state.CachedProjects.SingleOrDefault(p => p.Line == projectLine)
                      ?? throw new PebcakException($"local cache does not contain line #{projectLine}");

        var existingRow = _state.CachedTimeSheet.SingleOrDefault(r => r.Line == line)?.ChimpApiTimeSheetRecord
                          ?? throw new PebcakException($"local cache does not contain line #{line}");
        var updatedRow = existingRow with
        {
            CustomerId = project.CustomerId,
            ProjectId = project.ProjectId,
            ProjectTaskId = project.ProjectTaskId,
            Modified = DateTime.Now,
        };

        await ApiCall(HttpMethod.Put, "time/put", updatedRow);
    }
    
    public async Task DeleteRow(int line)
    {
        if (_state.CachedTimeSheet == null) throw new PebcakException("you cannot update timesheet data without first fetching the timesheet");

        var existingRecord = _state.CachedTimeSheet.SingleOrDefault(r => r.Line == line)?.ChimpApiTimeSheetRecord
                             ?? throw new PebcakException($"local cache does not contain line #{line}");
        await ApiCall(HttpMethod.Delete, $"time/delete?id={existingRecord.Id}", Array.Empty<int>());
    }

    private void PersistState() => ProtectedFileHelper.WriteProtectedFile(_stateFilePath, Util.JsonSerialize(_state));
    private StateData ReadPersistedState() => Util.JsonDeserialize<StateData>(ProtectedFileHelper.ReadProtectedFile(_stateFilePath), false);

    private async Task<string> ApiCall(HttpMethod method, string path, object? data = null)
    {
        if (_state.AuthToken == null) throw new ApplicationException("attempting to call api with no authtoken");
        
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Cookie", $".AspNet.ApplicationCookie={_state.AuthToken}");
        if (data != null)
        {
            request.Content = new StringContent(Util.JsonSerialize(data), new MediaTypeHeaderValue("application/json"));
        }

        var client = new HttpClient { BaseAddress = new Uri("https://app.timechimp.com/api/") };
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new ApiException($"got httpcode {response.StatusCode}: maybe need to re-authorize");
        var bodyString = await response.Content.ReadAsStringAsync();
        if (bodyString.TrimStart().StartsWith('<')) throw new ApiException($"api returned html: probably need to re-authorize");
        return bodyString;
    }
}
