using Chimp.Common;
using Chimp.Services;

namespace Chimp.Shell;
using static RenderHelper;

public class ListTimeSheetCommand : IShellCommand
{
    public async Task Handle(TimeSheetService service)
    {
        var timeSheet = await service.GetTimeSheet();

        var days = timeSheet.Rows.Select(r => r.TimeDetails.Date).Distinct().OrderBy(d => d);
        foreach (var date in days) RenderTimeSheetDay(timeSheet, date);

        if ( ! timeSheet.Date.IsCurrentWeek()) RenderTimeTravelerAlert(timeSheet.Date);
    }
}
