namespace Chimp;

public class ChimpListProjects(ChimpService service)
{
    public async Task Run()
    {
        var localizer = service.GetLocalizer();
        var projects = await service.GetProjects();
        var tags = await service.GetTags();

        Console.WriteLine();
        Console.WriteLine($"{$"{localizer.GetAvailableProjects()}:", -60} {localizer.GetAvailableTags()}:");
        for (var i = 0; i < Math.Max(projects.Count, tags.Count); i++)
        {
            var projectRow = projects.Count > i ? $"{ $"[{projects[i].Line}]",-4} {projects[i].Name}" : null;
            var tagRow = tags.Count > i ? $"{$"[{tags[i].Line}]",-4} {tags[i].Name}" : null;
            Console.WriteLine($"{projectRow, -60} {tagRow}");
        }
    }
}
