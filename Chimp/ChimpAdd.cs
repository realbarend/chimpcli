namespace Chimp;

public class ChimpAdd(ArgumentShifter args, ChimpService service)
{
    public async Task Run()
    {
        var projectLineString = args.GetString("project");
        if (!Util.TryParseProjectSpec(projectLineString, out var project, out var tags))
            throw new PebcakException("invalid project value, use pNN or pNN-A,B");

        var interval = args.GetString("timeSpec");
        var timeTravelingDate = service.GetTimeTravelingDate();
        var baseDate = timeTravelingDate ?? DateTime.Now;
        var timeInterval = new TimeInterval(interval, baseDate, service.GetLocalizer());
        if (timeTravelingDate != null && !timeInterval.InputContainsWeekDay) throw new PebcakException("when time traveling, the time interval must be prefixed with weekday");

        var notes = string.Join(" ", args.GetRemainingArgs());
        await service.AddRow(project, tags, timeInterval, notes);
    }
}
