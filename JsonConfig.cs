using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Zomlib;

public class JsonConfig
{
    public readonly JsonNode JObject;
    protected readonly string FilePath;
    protected readonly JsonConfig? Parent;

    public static JsonConfig Load(string path) => new(path, (JsonObject) JsonNode.Parse(File.ReadAllText(path)).ThrowIfNull());
    public static JsonConfig LoadOrCreate(string path)
    {
        if (!File.Exists(path))
            return new JsonConfig(path, new JsonObject());

        return Load(path);
    }

    public JsonConfig(string filepath, JsonNode jobject, JsonConfig? parent = null)
    {
        FilePath = filepath;
        JObject = jobject;
        Parent = parent;
    }

    public void Set<T>(string path, T value)
    {
        JObject[path] = value is JsonNode jn ? jn : JsonValue.Create(value);
        (Parent ?? this).Save();
    }
    void Save() => File.WriteAllText(FilePath, JObject.ToString());

    public T Get<T>(string path)
    {
        var value = JObject[path];

        if (typeof(T).IsAssignableTo(typeof(JsonNode)))
            return (T) (object) JObject[path]!;

        return JObject[path].Deserialize<T>()!;
    }
    public bool TryGet<T>(string path, [NotNullWhen(true)] out T? value)
    {
        try
        {
            value = Get<T>(path)!;
            return true;
        }
        catch
        {
            value = default;
            return false;
        }
    }

    [return: NotNullIfNotNull(nameof(defaultval))]
    public T? Get<T>(string path, T? defaultval)
    {
        var value = JObject[path];
        if (value is null) return defaultval;

        return Get<T>(path);
    }

    public T GetSet<T>(string path, T defaultset) => GetSet(path, () => defaultset);
    public T GetSet<T>(string path, Func<T> defaultset)
    {
        if (JObject[path] is null)
            Set(path, defaultset());

        return Get<T>(path);
    }

    public JsonConfig Object(string path) => new(FilePath, GetSet(path, () => new JsonObject()), Parent ?? this);
}