namespace Chimp;

public class ChimpDelete(ArgumentShifter args, ChimpService service)
{
    public async Task Run()
    {
        var line = args.GetInt32("line#");

        var localizer = service.GetLocalizer();
        Console.WriteLine(localizer.GetTimeSheetRow(service.GetCachedTimeSheetViewRow(line)));
        Console.WriteLine(localizer.TranslateLiteral("About to delete row #{Line}: are you sure? Y/N", new() { { "Line", line } }));
        if (Console.ReadKey(true).Key != localizer.GetYesKey())
        {
            Console.WriteLine(localizer.TranslateLiteral("Not removed."));
            Environment.Exit(1);
        }
        
        await service.DeleteRow(line);
    }
}
