namespace Chimp.Api.Models;

[Serializable]
public record ProjectTaskRecord
{
    public required long Id { get; init; }
    public required string Name { get; init; }
}
