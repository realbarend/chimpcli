namespace Chimp.DomainModels;

[Serializable]
public record Tag
{
    public required ShortId<Tag> ShortId { get; init; }
    public required long Id { get; init; }
    public required string Name { get; init; }
}
