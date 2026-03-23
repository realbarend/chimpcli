namespace Chimp.DomainModels;

public record User
{
    public required long Id { get; init; }
    public required string UserName { get; init; }
    public required string Language { get; init; }
}
