namespace Chimp;

public class ChimpAdd(ArgumentShifter args, IChimpService service)
{
    public async Task Run()
    {
        var projectSpec = args.GetString("projectSpec");
        if (!Util.TryParseProjectSpec(projectSpec, out var project, out var tags))
            throw new PebcakException("invalid projectSpec '{ProjectSpec}', use pNN or pNN-A,B", new() { { "ProjectSpec", projectSpec } });

        var remainingArgs = args.GetRemainingArgs();

        var timeSpecArg = DetermineTimeSpecArg(remainingArgs, out var timeInterval);
        var notes = string.Join(" ", remainingArgs.Where((value, index) => index != timeSpecArg));

        await service.AddRow(project, tags, timeInterval, notes);
    }

    private int DetermineTimeSpecArg(string[] remainingArgs, out TimeInterval timeInterval)
    {
        // args should be '<timeSpec> <comments>', '<comments> <timeSpec>' or just '<timeSpec>', so cannot be empty
        if (remainingArgs.Length > 0) foreach (var arg in (int[])[0, remainingArgs.Length - 1])
        {
            var timeSpec = remainingArgs[arg];
            var timeTravelingDate = service.GetTimeTravelingDate();
            var baseDate = timeTravelingDate ?? DateTime.Now;
            if ( !TimeInterval.TryParse(timeSpec, baseDate, service.GetLocalizer().ChimpCulture, out var interval)) continue;
            if (timeTravelingDate != null && ! TimeInterval.TryParseDayPrefix(timeSpec, service.GetLocalizer().ChimpCulture, out _))
                throw new PebcakException("when time traveling, the timeSpec must always include the weekday, eg 'fr:{TimeSpec}'", new() { { "TimeSpec", timeSpec } });
            timeInterval = interval;
            return arg;
        }

        throw new PebcakException("expected '{ParamName}' parameter missing", new() {{"ParamName", "timeSpec"}});
    }
}
