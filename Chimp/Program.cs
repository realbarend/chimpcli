using Chimp;

const string syntax = $"""
                       TimeChimp CLI for tracking hours
                       Get help at https://path.to.README
                       
                       Examples:
                       chimp login                                      # login with timechimp username and password
                       chimp projects                                   # list all projects you may work on
                       chimp add p12 9.30+15 my notes                   # started working on 'my notes' at 09:30, ended at 9:45
                       chimp                                            # list currently tracked hours for this week
                       chimp week -1                                    # change view to list previous week
                       chimp a p12 mo:9.30+15 note bla bla              # time may be prefixed with two-letter day reference
                       chimp add p13 14-15.30                           # started at 14:00, ended at 15.30, without notes
                       chimp update <line#> updating my note            # update the note
                       chimp u <line#> 930-10                           # update the time
                       chimp u <line#> fr:930-10                        # move this record to friday
                       chimp update <line#> p2                          # move this record to project 'p2'
                       chimp del <line#>                                # make this record be gone (will ask to confirm)
                       """;

var stateFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".chimpcli");
var service = new ChimpService(stateFilePath);
var shifter = new ArgumentShifter(args);

var defaultCommand = File.Exists(stateFilePath) ? "ls" : "login";
switch (shifter.GetString("command", defaultCommand))
{
    case "l": case "login":
        await new ChimpLogin(service).Run();
        await new ChimpListTimeSheet(service).Run();
        break;
    case "p": case "projects":
        await new ChimpListProjects(service).Run();
        break;
    case "ls": case "list":
        await new ChimpListTimeSheet(service).Run();
        break;
    case "w": case "week":
        var weekOffset = shifter.GetInt32("weekOffset", "0");
        await new ChimpListTimeSheet(service).Run(weekOffset);
        break;
    case "a": case "add":
        await new ChimpAdd(shifter, service).Run();
        await new ChimpListTimeSheet(service).Run();
        break;
    case "u": case "update":
        await new ChimpUpdate(shifter, service).Run();
        await new ChimpListTimeSheet(service).Run();
        break;
    case "d": case "del": case "delete":
        await new ChimpDelete(shifter, service).Run();
        await new ChimpListTimeSheet(service).Run();
        break;
    case "help": default:
        Console.WriteLine(syntax);
        return;
}
