namespace Chimp.Models.Api;

[Serializable]
public record ChimpApiTag(long CompanyId, string Name, bool Active, long Type, long Id);
