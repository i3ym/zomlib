namespace Zomlib.Commands;

public class MessageInfo
{
    public readonly ImmutableArray<object> Objects;
    public readonly ImmutableDictionary<string, object> NamedObjects;
    public readonly string Message;

    public MessageInfo(string message) : this(message, ImmutableArray<object>.Empty, ImmutableDictionary<string, object>.Empty) { }
    public MessageInfo(string message, IEnumerable<object>? objects = default, IEnumerable<KeyValuePair<string, object>>? namedObjects = default)
        : this(message, objects?.ToImmutableArray() ?? default, namedObjects?.ToImmutableDictionary()) { }
    public MessageInfo(string message, ImmutableArray<object> objects = default, ImmutableDictionary<string, object>? namedObjects = default)
    {
        Message = message;
        Objects = objects;
        NamedObjects = namedObjects ?? ImmutableDictionary<string, object>.Empty;
    }


    public MessageInfo WithText(string message) => new MessageInfo(message, Objects, NamedObjects);
    public MessageInfo WithObject<T>(T obj) where T : notnull => new MessageInfo(Message, Objects.Add(obj), NamedObjects);
    public MessageInfo WithObject<T>(string name, T obj) where T : notnull => new MessageInfo(Message, Objects, NamedObjects.SetItem(name, obj));

    public T Object<T>() => Objects.OfType<T>().First();
    public T? TryGetObject<T>() => Objects.OfType<T?>().FirstOrDefault();
    public T Object<T>(string name) => (T) NamedObjects[name];
    public T? TryGetObject<T>(string name)
    {
        if (NamedObjects.TryGetValue(name, out var obj) && obj is T t)
            return t;

        return default;
    }
}