namespace Chimp.Api.Models;

// NOTE: projects have some unmapped properties such as start/end date and customer name

[Serializable]
public record ProjectRecord
{
    public required long Id { get; init; }
    public required long CustomerId { get; init; }
    public required string Name { get; init; }
    public required bool Intern { get; init; }
}
