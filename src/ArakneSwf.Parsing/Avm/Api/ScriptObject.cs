using System.Collections;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArakneSwf.Parsing.Avm.Api;

/// <summary>
/// Base object class for ActionScript-like dynamic objects.
/// Supports plain properties, computed getters/setters, iteration, and JSON serialization.
/// </summary>
[JsonConverter(typeof(ScriptObjectJsonConverter))]
public class ScriptObject : IEnumerable<KeyValuePair<string, object?>>
{
    // Backing stores
    private readonly Dictionary<string, object?> _properties;
    private readonly Dictionary<string, Func<object?>> _getters;
    private readonly Dictionary<string, Action<object?>> _setters;

    public ScriptObject(
        Dictionary<string, object?>?         properties = null,
        Dictionary<string, Func<object?>>?   getters    = null,
        Dictionary<string, Action<object?>>? setters    = null)
    {
        _properties = properties ?? new Dictionary<string, object?>(StringComparer.Ordinal);
        _getters = getters ?? new Dictionary<string, Func<object?>>(StringComparer.Ordinal);
        _setters = setters ?? new Dictionary<string, Action<object?>>(StringComparer.Ordinal);
    }

    /// <summary>Total number of exposed properties (plain + computed).</summary>
    public int Count => _properties.Count + _getters.Count;

    /// <summary>
    /// Define a computed property.
    /// Returns false if the name is empty.
    /// </summary>
    public bool AddProperty(string name, Func<object?> getter, Action<object?>? setter = null)
    {
        if (string.IsNullOrEmpty(name)) return false;

        _getters[name] = getter ?? throw new ArgumentNullException(nameof(getter));
        if (setter is not null)
            _setters[name] = setter;

        return true;
    }

    /// <summary>Check if a property (plain or computed) exists.</summary>
    public bool Contains(string name)
        => !string.IsNullOrEmpty(name) && (_properties.ContainsKey(name) || _getters.ContainsKey(name));

    /// <summary>Remove a plain property (computed ones are not removed here).</summary>
    public bool Remove(string name) => _properties.Remove(name);

    // Indexers (string / int) to mimic PHP array-key behavior.
    public object? this[string name]
    {
        get => GetPropertyValue(name);
        set => SetPropertyValue(name, value);
    }

    public object? this[int index]
    {
        get => GetPropertyValue(index.ToString(CultureInfo.InvariantCulture));
        set => SetPropertyValue(index.ToString(CultureInfo.InvariantCulture), value);
    }

    /// <summary>
    /// Attempt to call a delegate stored as a property under <paramref name="name"/>.
    /// Returns null if not callable.
    /// </summary>
    public object? Call(string name, params object?[] args)
    {
        if (!_properties.TryGetValue(name, out var maybeDelegate) || maybeDelegate is not Delegate d)
            return null;

        return d.DynamicInvoke(args);
    }

    // --- IEnumerable over merged view (plain + computed) ---

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        foreach (var kv in _properties)
            yield return kv;

        foreach (var kv in _getters)
            yield return new KeyValuePair<string, object?>(kv.Key, SafeInvoke(kv.Value));
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // --- Internals ---

    private static object? SafeInvoke(Func<object?> getter)
    {
        // Match PHP behavior: just call; if it throws, let it bubble up.
        return getter();
    }

    private object? GetPropertyValue(string? name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        if (_getters.TryGetValue(name, out var getter))
            return SafeInvoke(getter);

        return _properties.TryGetValue(name, out var val) ? val : null;
    }

    private void SetPropertyValue(string? name, object? value)
    {
        if (string.IsNullOrEmpty(name)) return;

        if (_setters.TryGetValue(name, out var setter))
        {
            setter(value);
            return;
        }

        _properties[name] = value;
    }

    /// <summary>
    /// Build a dictionary snapshot (plain + computed) — used by the JSON converter.
    /// </summary>
    internal Dictionary<string, object?> ToMergedDictionary()
    {
        var result = new Dictionary<string, object?>(_properties, StringComparer.Ordinal);
        foreach (var (key, getter) in _getters)
            result[key] = SafeInvoke(getter);

        return result;
    }
}

/// <summary>
/// System.Text.Json converter that serializes ScriptObject as a JSON object
/// merging plain properties and computed getters.
/// </summary>
public sealed class ScriptObjectJsonConverter : JsonConverter<ScriptObject>
{
    public override ScriptObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Deserialization is not required by the PHP counterpart and is ambiguous
        // (no way to reconstruct getters/setters). We’ll support only serialization.
        throw new NotSupportedException("Deserializing ScriptObject is not supported.");
    }

    public override void Write(Utf8JsonWriter writer, ScriptObject value, JsonSerializerOptions options)
    {
        var map = value.ToMergedDictionary();
        writer.WriteStartObject();
        foreach (var kv in map)
        {
            writer.WritePropertyName(kv.Key);
            JsonSerializer.Serialize(writer, kv.Value, options);
        }
        writer.WriteEndObject();
    }
}
