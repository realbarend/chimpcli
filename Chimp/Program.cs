using Chimp;

var stateFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".chimpcli");
var service = new ChimpService(stateFilePath);
var shifter = new ArgumentShifter(args);
var defaultCommand = File.Exists(stateFilePath) ? "ls" : "login";

try
{
    switch (shifter.GetString("command", defaultCommand))
    {
        case "l":
        case "login":
            await new ChimpLogin(service).Run();
            await new ChimpListProjects(service).Run();
            await new ChimpListTimeSheet(service).Run();
            break;
        case "p":
        case "projects":
            await new ChimpListProjects(service).Run();
            break;
        case "ls":
        case "list":
            await new ChimpListTimeSheet(service).Run();
            break;
        case "w":
        case "week":
            var weekOffset = shifter.GetInt32("weekOffset", "0");
            await new ChimpListTimeSheet(service).Run(weekOffset);
            break;
        case "a":
        case "add":
            await new ChimpAdd(shifter, service).Run();
            await new ChimpListTimeSheet(service).Run();
            break;
        case "u":
        case "update":
            await new ChimpUpdate(shifter, service).Run();
            await new ChimpListTimeSheet(service).Run();
            break;
        case "d":
        case "del":
        case "delete":
            await new ChimpDelete(shifter, service).Run();
            await new ChimpListTimeSheet(service).Run();
            break;
        case "help":
        default:
            Console.WriteLine(service.GetLocalizer().GetHelpMessage());
            return;
    }
}
catch (LocalizedException le)
{
    var localizer = service.GetLocalizer();
    Console.Error.WriteLine($"{le.GetType().Name}: {localizer.TranslateLiteral(le.Message, le.Args)}");
    Console.Error.WriteLine(le.StackTrace);
    Console.Error.WriteLine();
    Console.Error.WriteLine(localizer.TranslateLiteral("try 'chimp help' to get help"));
    Environment.Exit(1);
}
