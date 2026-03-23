namespace Chimp.DomainModels;

[Serializable]
public record ProjectTask
{
    public required ShortId<ProjectTask> ShortId { get; init; }
    public required long Id { get; init; }
    public required long ProjectId { get; init; }
    public required long CustomerId { get; init; }
    public required string ProjectName { get; init; }
    public required string TaskName { get; init; }
}
