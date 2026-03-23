using System.Text.Json;

namespace Chimp.Common;

public class PersistablePropertyBag
{
    private string? _diskPath;
    private Dictionary<string, object?> _objects = new();

    public static PersistablePropertyBag CreateNew(string path)
    {
        return new PersistablePropertyBag { _diskPath = path };
    }

    public static PersistablePropertyBag ReadFromDisk(string path)
    {
        return new PersistablePropertyBag
        {
            _diskPath = path,
            _objects = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                ProtectedFileHelper.ReadProtectedFile(path)) ?? [],
        };
    }

    public T? Get<T>()
    {
        if (!_objects.TryGetValue(typeof(T).Name, out var propertyValue)) return default;

        return propertyValue switch
        {
            JsonElement jsonElement => jsonElement.Deserialize<T>(),
            T value => value,
            _ => throw new ApplicationException($"cannot read {typeof(T).Name} from state"),
        };
    }

    public void Set<T>(T? obj) => _objects[typeof(T).Name] = obj;

    public void Delete<T>() => _objects.Remove(typeof(T).Name);

    public void Save()
    {
        if (_diskPath != null) ProtectedFileHelper.WriteProtectedFile(_diskPath, JsonSerializer.Serialize(_objects));
    }
}
