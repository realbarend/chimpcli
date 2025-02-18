// ReSharper disable RedundantAnonymousTypePropertyName
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
        if (_state.Auth?.LoginPassword == null) return false;
        Console.WriteLine($"Logging in using previously persisted credentials");
        try
        {
            await DoLogin(_state.Auth.LoginUserName, _state.Auth.LoginPassword, true);
            return true;
        }
        catch
        {
            Console.WriteLine("** login using persisted credentials failed!");
            throw;
        }
    }

    public async Task DoLogin(string userName, string password, bool persistCredentials)
    {
        try
        {
            var authResult = await AuthHelper.Login(userName, password);
            _state.Auth = new AuthProperties
            {
                LoginUserName = userName,
                LoginPassword = persistCredentials ? password : null,
                CognitoUserPoolId = authResult.Environment.UserPoolId,
                CognitoClientId = authResult.Environment.ClientId,
                AccessToken = authResult.AccessToken,
                Expires = authResult.Expires,
                RefreshToken = authResult.RefreshToken,
            };
        }
        catch (Exception e) // Amazon.CognitoIdentityProvider.Model.NotAuthorizedException: Incorrect username or password
        {
            throw new PebcakException("login attempt failed, maybe wrong password. note: timechimp can temporarily block your account after multiple failures ({Message})", new() {{"Message", e.Message}});
        }

        _state.User = Util.JsonDeserialize<ChimpApiUser>(await ApiCall(HttpMethod.Get, "user/current"), false);
        PersistState();
        var localizer = GetLocalizer();
        Console.WriteLine(localizer.TranslateLiteral("** login successful, got accesstoken valid until {ExpireDate}, will automatically refresh using refreshtoken", new() {{"ExpireDate",_state.Auth.Expires.ToLocalTime().ToString("D", localizer.ChimpCulture)}}));
    }

    public Localizer GetLocalizer()
    {
        return new Localizer(Environment.GetEnvironmentVariable("CHIMPCLI_LANGUAGE") ?? _state.User?.Language ?? "en");
    }

    public async Task<List<ProjectViewModel>> GetProjects()
    {
        if (_state.User == null) throw new PebcakException("you must first login");
        _state.CachedProjects = null;
        return await GetProjectsCached();
    }

    private async Task<List<ProjectViewModel>> GetProjectsCached(bool fetchIfNotCached = true)
    {
        if (_state.User == null) throw new PebcakException("you must first login");
        if (_state.CachedProjects == null)
        {
            if (!fetchIfNotCached) throw new PebcakException("you must first fetch the project list");
            var projects = Util.JsonDeserialize<List<ChimpApiProject>>(await ApiCall(HttpMethod.Get, $"project/{_state.User.Id}/uiselectbyuser"), false).OrderBy(p => p.Intern).ThenBy(p => p.Name).ToList();
            _state.CachedProjects = new List<ProjectViewModel>();
            foreach (var project in projects)
            {
                foreach (var task in Util.JsonDeserialize<List<ChimpApiProjectTask>>(
                             await ApiCall(HttpMethod.Get, $"projecttask/uiselect%2Fproject/{project.Id}"), false)
                             .OrderBy(t => t.Name))
                {
                    _state.CachedProjects.Add(new ProjectViewModel(_state.CachedProjects.Count + 1, project, task));
                }
            }
            PersistState();
        }

        return _state.CachedProjects;
    }

    public async Task<List<TagViewModel>> GetTags()
    {
        if (_state.User == null) throw new PebcakException("you must first login");
        _state.CachedTags = null;
        return await GetTagsCached();
    }

    private async Task<List<TagViewModel>> GetTagsCached(bool fetchIfNotCached = true)
    {
        if (_state.User == null) throw new PebcakException("you must first login");
        if (_state.CachedTags == null)
        {
            if (!fetchIfNotCached) throw new PebcakException("you must first fetch the project list");
            _state.CachedTags = Util.JsonDeserialize<List<ChimpApiTag>>(await ApiCall(HttpMethod.Get, $"tag/type%2F1"))
                .OrderBy(t => t.Name)
                .Select((t, idx) => new TagViewModel(idx + 1, t)).ToList();

            PersistState();
        }

        return _state.CachedTags;
    }

    public async Task<(double WeekTotal, double BillableTotal, List<TimeSheetRowViewModel> TimeSheet)> GetTimeSheet(int? weekOffset)
    {
        if (_state.User == null) throw new PebcakException("you must first login");
        ProcessWeekOffset();

        var responseBody = await ApiCall(HttpMethod.Get, $"time/week/{_state.User.Id}/{_state.TimeTravelingDate ?? DateTime.Now:yyyy-MM-dd}");
        var data = Util.JsonDeserialize<List<ChimpApiTimeSheetRecord>>(responseBody);

        var projects = await GetProjectsCached();
        var tags = await GetTagsCached();
        _state.CachedTimeSheet = data
            .OrderBy(row => row.Date).ThenBy(r => r.Start)
            .Select((row, idx) => TimeSheetRowViewModel.FromApiModel(row, idx + 1, projects, tags))
            .ToList();
        PersistState();
        return (data.Sum(r => r.Hours), data.Where(d => d.Billable).Sum(r => r.Hours), _state.CachedTimeSheet);

        void ProcessWeekOffset()
        {
            if (weekOffset != null)
            {
                if (weekOffset is < -52 or > 52) throw new PebcakException("invalid weekOffset: time travel is allowed for maximum 52 weeks");
                _state.TimeTravelingDate = weekOffset == 0 ? null : DateTime.Now.AddDays(7 * weekOffset.Value);
            }

            if (_state.TimeTravelingDate != null && Util.GetFirstDayOfWeek(_state.TimeTravelingDate.Value) == Util.GetFirstDayOfWeek(DateTime.Today))
            {
                // user was previously time traveling, but appears to have arrived in current time, so we can reset
                _state.TimeTravelingDate = null;
            }
        }
    }

    public TimeSheetRowViewModel GetCachedTimeSheetViewRow(int line)
    {
        if (_state.CachedTimeSheet == null) throw new PebcakException("you must first fetch the timesheet");
        return _state.CachedTimeSheet.SingleOrDefault(r => r.Line == line)
               ?? throw new PebcakException("previously fetched timesheet does not contain line #{Line}", new() {{"Line",line}});
    }

    public DateTime? GetTimeTravelingDate() => _state.TimeTravelingDate;

    public async Task AddRow(int projectLine, IEnumerable<int> tagLines, TimeInterval interval, string notes)
    {
        if (_state.User == null) throw new PebcakException("you must first login");

        var projects = await GetProjectsCached(false);
        var tags = await GetTagsCached(false);
        var project = projects.GetProjectByLine(projectLine);
        var tagIds = tags.MapTagLinesToIds(tagLines);

        await ApiCall(HttpMethod.Post, "time", new
        {
            UserId = _state.User.Id,
            CustomerId = project.ApiProject.CustomerId,
            ProjectId = project.ApiProject.Id,
            ProjectTaskId = project.ApiProjectTask.Id,
            TagIds = tagIds,
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
        var existingRecord = GetCachedTimeSheetViewRow(line).ApiTimeSheetRecord;
        await ApiCall(HttpMethod.Put, $"time/{existingRecord.Id}", existingRecord with
        {
            Notes = notes,
            Modified = DateTime.Now,
        });
    }

    public async Task UpdateTimeInterval(int line, TimeInterval interval)
    {
        var existingRecord = GetCachedTimeSheetViewRow(line).ApiTimeSheetRecord;
        await ApiCall(HttpMethod.Put, $"time/{existingRecord.Id}", existingRecord with
        {
            Date = interval.Start,
            Start = interval.Start,
            End = interval.End,
            Hours = (interval.End - interval.Start).TotalHours,
            StartEnd = $"{interval.Start:HH:mm}-{interval.End:HH:mm}",
            Modified = DateTime.Now,
        });
    }

    public async Task UpdateProject(int line, int projectLine, IEnumerable<int> tagLines)
    {
        var projects = await GetProjectsCached(false);
        var tags = await GetTagsCached(false);
        var project = projects.GetProjectByLine(projectLine);
        var tagIds = tags.MapTagLinesToIds(tagLines);

        var existingRecord = GetCachedTimeSheetViewRow(line).ApiTimeSheetRecord;
        await ApiCall(HttpMethod.Put, $"time/{existingRecord.Id}", existingRecord with
        {
            CustomerId = project.ApiProject.CustomerId,
            ProjectId = project.ApiProject.Id,
            ProjectTaskId = project.ApiProjectTask.Id,
            TagIds = tagIds,
            Modified = DateTime.Now,
        });
    }

    public async Task DeleteRow(int line)
    {
        var existingRecord = GetCachedTimeSheetViewRow(line).ApiTimeSheetRecord;
        await ApiCall(HttpMethod.Delete, $"time/{existingRecord.Id}", Array.Empty<int>());
    }

    private void PersistState() => ProtectedFileHelper.WriteProtectedFile(_stateFilePath, Util.JsonSerialize(_state));
    private StateData ReadPersistedState() => Util.JsonDeserialize<StateData>(ProtectedFileHelper.ReadProtectedFile(_stateFilePath), false);

    private async Task<string> ApiCall(HttpMethod method, string path, object? data = null)
    {
        if (_state.Auth == null) throw new ApplicationException("attempted to call api with no authtoken");
        await CheckRefreshToken();

        using var request = new HttpRequestMessage(method, path);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _state.Auth.AccessToken);
        if (data != null) request.Content = new StringContent(Util.JsonSerialize(data), new MediaTypeHeaderValue("application/json"));
        var client = new HttpClient { BaseAddress = new Uri("https://web.timechimp.com/api/") };
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new ApiException($"got httpcode {(int)response.StatusCode} ({response.StatusCode}): maybe need to re-authorize");
        var bodyString = await response.Content.ReadAsStringAsync();
        if (bodyString.TrimStart().StartsWith('<')) throw new ApiException($"api returned html: probably need to re-authorize");
        return bodyString;
    }

    private async Task CheckRefreshToken()
    {
        if (_state.Auth == null || _state.Auth.Expires > DateTimeOffset.UtcNow.AddSeconds(60)) return;

        var newAuth = await AuthHelper.TryRefresh(_state.Auth);
        if (newAuth == null) return;

        _state.Auth.AccessToken = newAuth.AccessToken;
        _state.Auth.Expires = newAuth.Expires;
        _state.Auth.RefreshToken = newAuth.RefreshToken;
        PersistState();
        Console.WriteLine($"** refresh: got new accesstoken valid until {_state.Auth.Expires.ToLocalTime()}");
    }
}
