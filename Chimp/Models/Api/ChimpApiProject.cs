namespace Chimp.Models.Api;

[Serializable]
public record ChimpApiProject(long Id, long CustomerId, string? Name, bool Intern);
