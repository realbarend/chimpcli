using JetBrains.Annotations;

namespace Chimp.Models.Api;

[UsedImplicitly]
public record ChimpApiUser(long Id, string UserName, string Language, double ContractHours);
