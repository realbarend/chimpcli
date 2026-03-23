namespace Chimp.DomainModels;

[Serializable]
public record ShortId<T>(int Value)
{
    public string Type { get; } = typeof(T).ToString();
    public override string ToString() => Value.ToString();
}
