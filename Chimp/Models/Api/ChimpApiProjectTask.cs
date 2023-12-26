using JetBrains.Annotations;

namespace Chimp.Models.Api;

[UsedImplicitly]
public class ChimpApiProjectTask
{
    public long Id { get; set; }
    public string? Name { get; set; }
}
