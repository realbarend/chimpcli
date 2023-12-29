namespace Chimp.Models.Api;

[Serializable]
public record ChimpApiUser(long Id, string UserName, string Language, double ContractHours);
