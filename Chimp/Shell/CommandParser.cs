using Chimp.Common;
using Chimp.DomainModels;

namespace Chimp.Shell;

public class CommandParser(IEnvironment environment, DebugLogger logger)
{
    public IShellCommand ParseCommandLine()
    {
        var args = environment.GetCommandLineArgs().Skip(1).ToList();

        switch (ShiftArgument(args, "command", "ls"))
        {
            case "l":
            case "login":
                return ParseLoginCommand();
            case "p":
            case "projects":
                return new ListProjectsCommand();
            case "ls":
            case "list":
                return new ListTimeSheetCommand();
            case "w":
            case "week":
                var weekOffset = ShiftNumberArgument(args, "weekOffset", 0);
                return new TimeTravelerCommand { WeekOffset = weekOffset };
            case "a":
            case "add":
                return ParseAddCommand(args);
            case "u":
            case "update":
                return ParseUpdateCommand(args);
            case "d":
            case "del":
            case "delete":
                var shortId = ShiftNumberArgument(args, "number#");
                return new DeleteTimeSheetRowCommand { ShortId = new ShortId<TimeSheetRow>(shortId) };
            case "c":
            case "copy":
                return ParseCopyCommand(args);
            case "help":
            default:
                return new HelpCommand();
        }
    }

    private static AddTimeSheetRowCommand ParseAddCommand(List<string> args)
    {
        var project = ShiftArgument(args, "project");
        if (!RenderHelper.TryParseProjectAlias(project, out var projectShortId, out var tagShortIds))
            throw new Error("invalid project '{ProjectAlias}', use pNN or pNN-A,B", new { ProjectAlias = project });

        var remainingAddArgs = args.ToArray();

        // args should be '<timeEntry> <comments>', '<comments> <timeEntry>' or just '<timeEntry>', so cannot be empty
        if (remainingAddArgs.Length == 0)
            throw new Error("expected '{ParamName}' parameter missing", new { ParamName = "timeEntry" });

        if (TimeEntry.TryParse(remainingAddArgs[0], out var timeEntry))
            return new AddTimeSheetRowCommand { Project = projectShortId, Tags = tagShortIds, TimeEntry = timeEntry, Notes = string.Join(" ", remainingAddArgs[1..]) };

        if (TimeEntry.TryParse(remainingAddArgs[^1], out timeEntry))
            return new AddTimeSheetRowCommand { Project = projectShortId, Tags = tagShortIds, TimeEntry = timeEntry, Notes = string.Join(" ", remainingAddArgs[..^1]) };

        throw new Error("expected '{ParamName}' parameter missing", new { ParamName = "timeEntry" });
    }

    private static IShellCommand ParseUpdateCommand(List<string> args)
    {
        var shortId = ShiftNumberArgument(args, "number#");

        var remainingArgs = args.ToArray();
        var nextArg = remainingArgs.FirstOrDefault();

        if (nextArg == null) throw new Error("expected '{ParamName}' parameter missing", new { ParamName = "new-notes" });

        if (RenderHelper.TryParseProjectAlias(nextArg, out var projectShortId, out var tagShortIds))
            return new UpdateTimeSheetRowCommand { ShortId = new ShortId<TimeSheetRow>(shortId), Project = projectShortId, Tags = tagShortIds };

        if (TimeEntry.TryParse(nextArg, out var timeEntry))
            return new UpdateTimeSheetRowCommand { ShortId = new ShortId<TimeSheetRow>(shortId), TimeEntry = timeEntry };

        return new UpdateTimeSheetRowCommand { ShortId = new ShortId<TimeSheetRow>(shortId), Notes = string.Join(" ", remainingArgs) };
    }

    private static CopyTimeSheetRowCommand ParseCopyCommand(List<string> args)
    {
        var shortId = ShiftNumberArgument(args, "number#");
        return TimeEntry.TryParse(ShiftArgument(args, "timeEntry"), out var timeEntry)
            ? new CopyTimeSheetRowCommand { ShortId = new ShortId<TimeSheetRow>(shortId), TimeEntry = timeEntry }
            : throw new Error("expected '{ParamName}' parameter missing", new { ParamName = "timeEntry" });
    }

    private LoginCommand ParseLoginCommand()
    {
        var persistCredentials = environment.GetBoolean(environment.ChimpCliStorePassword);
        var user = environment.GetEnvironmentVariable(environment.ChimpCliUserName);
        var password = environment.GetEnvironmentVariable(environment.ChimpCliPassword);

        if (persistCredentials) logger.Log($"** reading storePassword=true from env.{environment.ChimpCliStorePassword}");
        if ( ! string.IsNullOrEmpty(user)) logger.Log($"** reading your timechimp username from env.{environment.ChimpCliUserName}");
        if ( ! string.IsNullOrEmpty(password)) logger.Log($"** reading your timechimp password from env.{environment.ChimpCliPassword}");

        return new LoginCommand { User = user, Password = password, PersistCredentials = persistCredentials };
    }

    private static string ShiftArgument(List<string> args, string argName, string? defaultValue = null)
    {
        if (args.Count <= 0) return defaultValue ?? throw new Error("expected '{ParamName}' parameter missing", new { ParamName = argName });

        var arg = args[0];
        args.RemoveAt(0);
        return arg;
    }

    private static int ShiftNumberArgument(List<string> args, string argName, int? defaultValue = null)
    {
        var numberStr = ShiftArgument(args, argName, defaultValue?.ToString());
        return int.TryParse(numberStr, out var value)
            ? value
            : throw new Error("parameter '{ParamName}' must be a number", new { ParamName = argName });
    }
}
