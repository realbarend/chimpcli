namespace Chimp;

public class ChimpListProjects(ChimpService service)
{
    public async Task Run()
    {
        var projects = await service.GetProjects();

        Console.WriteLine();
        foreach (var project in projects)
        {
            Console.WriteLine($"{ $"[{project.Line}]",-4} {project.ProjectName} {project.TaskName}");
        }
        Console.WriteLine();
    }
}
