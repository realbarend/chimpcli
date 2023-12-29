using Chimp.Models.Api;

namespace Chimp.Models;

[Serializable]
public record ProjectViewModel(int Line, ChimpApiProject ApiProject, ChimpApiProjectTask ApiProjectTask)
{
    public string Name { get; } = $"{ApiProjectTask.Name} ({ApiProject.Name})";
}
