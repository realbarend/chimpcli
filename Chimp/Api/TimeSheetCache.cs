using Chimp.Api.Models;
using Chimp.Common;

namespace Chimp.Api;

public class TimeSheetCache
{
    // TODO Could we briefly cache to disk for say 2 minutes. Or as long as the parent process id stays the same?

    private readonly Dictionary<DateOnly, Dictionary<long, TimeSheetRecord>> _timeSheetRecords = new();

    public void Clear() => _timeSheetRecords.Clear();

    public async Task<Dictionary<long, TimeSheetRecord>> GetOrUpdateRecords(DateOnly date, Func<Task<Dictionary<long, TimeSheetRecord>>> fetchRecords)
    {
        if (_timeSheetRecords.TryGetValue(date.WeekStart, out var cachedRecords)) return cachedRecords;

        cachedRecords = await fetchRecords();
        _timeSheetRecords[date] = cachedRecords;
        return cachedRecords;
    }

    public Dictionary<long, TimeSheetRecord>? GetRecords(DateOnly date) => _timeSheetRecords.GetValueOrDefault(date.WeekStart);
}
