namespace Chimp;

public class ChimpUpdate(ArgumentShifter args, IChimpService service)
{
    public async Task Run()
    {
        var line = args.GetInt32("line#");
        var baseDate = service.GetCachedTimeSheetViewRow(line).Date;

        // args parsing is a bit of a hassle because the next arg may be the first word of the 'notes' parameter
        var remainingArgs = args.GetRemainingArgs();
        var nextArg = remainingArgs.FirstOrDefault();

        if (nextArg == null)
        {
            await service.UpdateNotes(line, string.Empty);
        }
        else if (Util.TryParseProjectSpec(nextArg, out var project, out var tags))
        {
            await service.UpdateProject(line, project, tags);
        }
        else if (TimeInterval.TryParse(nextArg, baseDate, service.GetLocalizer().ChimpCulture, out var timeInterval))
        {
            await service.UpdateTimeInterval(line, timeInterval);
        }
        else
        {
            await service.UpdateNotes(line, string.Join(" ", remainingArgs));
        }
    }
}
