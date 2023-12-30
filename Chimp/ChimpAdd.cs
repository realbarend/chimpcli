namespace Chimp;

public class ChimpAdd(ArgumentShifter args, ChimpService service)
{
    public async Task Run()
    {
        var projectSpec = args.GetString("projectSpec");
        if (!Util.TryParseProjectSpec(projectSpec, out var project, out var tags))
            throw new PebcakException("invalid projectSpec '{ProjectSpec}', use pNN or pNN-A,B", new() { { "ProjectSpec", projectSpec } });

        var timeSpec = args.GetString("timeSpec");
        var timeTravelingDate = service.GetTimeTravelingDate();
        var baseDate = timeTravelingDate ?? DateTime.Now;
        var timeInterval = new TimeInterval(timeSpec, baseDate, service.GetLocalizer().ChimpCulture);
        if (timeTravelingDate != null && !timeInterval.InputContainsWeekDay) throw new PebcakException("when time traveling, the timeSpec must always include the weekday, eg 'fr:{TimeSpec}'", new() { { "TimeSpec", timeSpec } });

        var notes = string.Join(" ", args.GetRemainingArgs());
        await service.AddRow(project, tags, timeInterval, notes);
    }
}
