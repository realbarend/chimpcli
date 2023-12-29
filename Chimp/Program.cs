using Chimp;

const string syntax = $"""
                       TimeChimp CLI for tracking hours
                       Get more information at https://github.com/realbarend/chimpcli

                       Parameters:
                       login, l                                   # login with timechimp username and password
                       projects, p                                # list all projects and tags you may work on
                       add, a <projectspec> <timespec> <notes>    # add a new record
                       ls (this is the default)                   # list currently tracked hours for this week
                       week, w <week-offset>                      # change the view to work on a different week
                       update, u <line#> <new-notes>              # update notes for existing record
                       update, u <line#> <new-timespec>           # update timespec for existing record
                       update, u <line#> <new-projectspec>        # set different project and/or tags
                       delete, del, d <line#>                     # make the record be gone (will ask to confirm)
                       
                       <projectspec>: pXX                         # specifies the project by number (see 'projects' command)
                       <projectspec>: pXX-A or pXX-A,B            # projectspec may include tagnumbers A,B
                       <timespec>: from-to                        # specifies time-interval as 8.30-10.00
                       <timespec>: from+minutes                   # specifies time-interval as 8.30+30 -> 8.30-9.00
                       <timespec>: dd:from-to or dd:from+minutes  # specifies different day, eg 'fr:' sets or moves to Friday
                       
                       Examples:
                       chimp login                                # login with timechimp username and password
                       chimp projects                             # list all projects and tags you may work on
                       chimp add p12 9.30+15 my notes             # started working on 'my notes' at 09:30, ended at 9:45
                       chimp a p12 mo:9.30+15 note bla bla        # time may be prefixed with two-letter day reference
                       chimp add p13 14-15:30                     # started at 14:00, ended at 15.30, without notes
                       chimp                                      # list currently tracked hours for this week
                       chimp week -1                              # change view to list previous week
                       chimp update 5 updating my note            # update the note for the fifth record
                       chimp u 5 930-10                           # update the time for the fifth record
                       chimp u 5 fr:930-10                        # move fifth record to friday
                       chimp update 5 p2-3,5                      # move fifth record to project 'p2' and set tag '3' and '5'
                       chimp del 5                                # make fifth record be gone (will ask to confirm)
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
