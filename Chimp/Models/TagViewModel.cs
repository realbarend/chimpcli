using Chimp.Models.Api;

namespace Chimp.Models;

[Serializable]
public record TagViewModel(int Line, ChimpApiTag ApiTag)
{
    public string Name { get; } = ApiTag.Name;
}
