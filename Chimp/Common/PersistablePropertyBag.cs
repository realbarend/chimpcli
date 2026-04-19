using System.Text.Json;

namespace Chimp.Common;

public class PersistablePropertyBag
{
    private string? _diskPath;
    private Dictionary<string, JsonElement> _objects = new();

    public static PersistablePropertyBag CreateNew(string path)
    {
        return new PersistablePropertyBag { _diskPath = path };
    }

    public static PersistablePropertyBag ReadFromDisk(string path)
    {
        var data = ProtectedFileHelper.ReadProtectedFile(path);
        return new PersistablePropertyBag
        {
            _diskPath = path,
            _objects = JsonContext.Deserialize<Dictionary<string, JsonElement>>(data) ?? [],
        };
    }

    public T? Get<T>()
    {
        return _objects.TryGetValue(typeof(T).Name, out var element)
            ? JsonContext.DeserializeFromElement<T>(element)
            : default;
    }

    public void Set<T>(T? obj)
    {
        if (obj is null) _objects.Remove(typeof(T).Name);
        else _objects[typeof(T).Name] = JsonContext.SerializeToElement(obj);
    }

    public void Delete<T>() => _objects.Remove(typeof(T).Name);

    public void Save()
    {
        ProtectedFileHelper.WriteProtectedFile(
            _diskPath ?? throw new InvalidOperationException("Disk path is not set"),
            JsonContext.Serialize(_objects));
    }
}
