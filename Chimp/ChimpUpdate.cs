namespace Chimp;

public class ChimpUpdate(ArgumentShifter args, ChimpService service)
{
    public async Task Run()
    {
        var line = args.GetInt32("line");
        var baseDate = service.GetCachedTimeSheetViewRow(line).Date;

        // args parsing is a bit of a hassle because the next arg may be the first word of the 'notes' parameter
        var remainingArgs = args.GetRemainingArgs();
        var nextArg = remainingArgs.FirstOrDefault();
        if (nextArg != null)
        {
            if (Util.TryParseProjectSpec(nextArg, out var project, out var tags))
            {
                await service.UpdateProject(line, project, tags);
                return;
            }

            try
            {
                var interval = new TimeInterval(nextArg, baseDate, service.GetLocalizer());
                await service.UpdateTimeInterval(line, interval);
                return;
            }
            catch { /* parsing the arg as a timeinterval failed: we now assume the arg is a note */ }
        }

        var notes = string.Join(" ", remainingArgs);
        await service.UpdateNotes(line, notes);
    }
}
