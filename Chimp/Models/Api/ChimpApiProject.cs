using JetBrains.Annotations;

namespace Chimp.Models.Api;

[UsedImplicitly]
public class ChimpApiProject
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string? Name { get; set; }
    public bool Intern { get; set; }
}