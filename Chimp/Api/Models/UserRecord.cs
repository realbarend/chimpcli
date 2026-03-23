using Chimp.DomainModels;

namespace Chimp.Api.Models;

[Serializable]
public record UserRecord
{
    public required long Id { get; init; }
    public required string UserName { get; init; }
    public required string Language { get; init; }
    public required double ContractHours { get; init; }

    public User ToUser() => new() { Id = Id, UserName = UserName, Language = Language };
}
