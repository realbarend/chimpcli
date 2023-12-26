namespace Chimp;

public class ChimpDelete(ArgumentShifter args, ChimpService service)
{
    public async Task Run()
    {
        var line = args.GetInt32("line");

        var localizer = service.GetLocalizer();
        Console.WriteLine(localizer.GetTimeSheetRow(service.GetCachedTimeSheetViewRow(line)));
        Console.WriteLine(localizer.GetDeleteConfirmation(line));
        if (Console.ReadKey(true).Key != localizer.GetYesKey())
        {
            Console.WriteLine(localizer.GetDeleteAborted());
            Environment.Exit(1);
        }
        
        await service.DeleteRow(line);
    }
}
