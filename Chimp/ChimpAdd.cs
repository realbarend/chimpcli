using System.Text.RegularExpressions;

namespace Chimp;

public class ChimpAdd(ArgumentShifter args, ChimpService service)
{
    public async Task Run()
    {
        var projectLineString = args.GetString("project");
        var projectMatch = Regex.Match(projectLineString, @"^p(?<Project>\d{1,2})$");
        if (!projectMatch.Success) throw new PebcakException("invalid project value, use pNN");
        var projectLine = int.Parse(projectMatch.Groups["Project"].Value);

        var interval = args.GetString("timeSpec");
        var timeTravelingDate = service.GetTimeTravelingDate();
        var baseDate = timeTravelingDate ?? DateTime.Now;
        var timeInterval = new TimeInterval(interval, baseDate, service.GetLocalizer());
        if (timeTravelingDate != null && !timeInterval.InputContainsWeekDay) throw new PebcakException("when time traveling, the time interval must be prefixed with weekday");

        var notes = string.Join(" ", args.GetRemainingArgs());
        await service.AddRow(projectLine, timeInterval, notes);
    }
}
