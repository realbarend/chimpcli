using Chimp.Services;

namespace Chimp.Shell;

public class ListProjectsCommand : IShellCommand
{
    public async Task Handle(TimeSheetService service)
    {
        service.ClearProjectsCache();
        var timeSheet = await service.GetTimeSheet();

        Console.WriteLine();
        Console.WriteLine($"{$"{Localization.Localize("Available projects")}:", -60} {Localization.Localize("Available tags")}:");
        for (var i = 0; i < Math.Max(timeSheet.ProjectTasks.Length, timeSheet.Tags.Length); i++)
        {
            var projectRow = timeSheet.ProjectTasks.Length > i ? $"{ $"[{i + 1}]",-4} {timeSheet.ProjectTasks[i].TaskName} ({timeSheet.ProjectTasks[i].ProjectName})" : null;
            var tagRow = timeSheet.Tags.Length > i ? $"{$"[{i + 1}]",-4} {timeSheet.Tags[i].Name}" : null;
            Console.WriteLine($"{projectRow, -60} {tagRow}");
        }
    }
}
