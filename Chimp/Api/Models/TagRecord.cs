using System.Text.Json.Serialization;

namespace Chimp.Api.Models;

// Active and Type do not seem to be used actively: all tags are active and type 1.

[Serializable]
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public record TagRecord
{
    public required long CompanyId { get; init; }
    public required string Name { get; init; }
    public required bool Active { get; init; }
    public required long Type { get; init; }
    public required long Id { get; init; }
}
