using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArakneSwf.Parsing.Avm.Api;

[JsonConverter(typeof(ScriptArray.ScriptArrayJsonConverter))]
public class ScriptArray : ScriptObject, IEnumerable<object?>
{
    private readonly List<object?> _values;

    /// <summary>
    /// Create an array. If a single integer is provided, pre-allocates with nulls (AS Array(n)).
    /// </summary>
    public ScriptArray(params object?[] values)
        : base()
    {
        if (values.Length == 1 && values[0] is int n)
        {
            _values = new List<object?>(capacity: n);
            for (int i = 0; i < n; i++) _values.Add(null);
        }
        else
        {
            _values = new List<object?>(values);
        }

        // Expose computed "length" like in PHP: getter + setter -> SetLength
        AddProperty(
            "length",
            getter: () => _values.Count,
            setter: v => SetLength(CoerceToInt(v))
        );
    }

    /// <summary>Numeric indexer (AS array-style). Extends array with nulls if needed.</summary>
    public new object? this[int index]
    {
        get => (index >= 0 && index < _values.Count) ? _values[index] : base[index];
        set
        {
            if (index < 0)
            {
                base[index] = value;
                return;
            }

            EnsureCapacity(index + 1);
            _values[index] = value;
        }
    }

    /// <summary>
    /// Equivalent “unset” pour un index numérique: place la valeur à null.
    /// </summary>
    public void UnsetAt(int index)
    {
        if (index >= 0 && index < _values.Count)
            _values[index] = null;
    }

    /// <summary>Iterate values only (like PHP getIterator())</summary>
    System.Collections.Generic.IEnumerator<object?> System.Collections.Generic.IEnumerable<object?>.GetEnumerator()
        => _values.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => _values.GetEnumerator();

    // ---- helpers ----

    private void EnsureCapacity(int required)
    {
        while (_values.Count < required)
            _values.Add(null);
    }

    private static int CoerceToInt(object? v)
    {
        if (v is null) return 0;
        return v switch
        {
            int i => i,
            long l => (int)l,
            short s => s,
            byte b => b,
            double d => (int)d,
            float f => (int)f,
            decimal m => (int)m,
            string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) => i,
            _ => Convert.ToInt32(v, CultureInfo.InvariantCulture)
        };
    }

    private void SetLength(int length)
    {
        if (length <= 0)
        {
            _values.Clear();
            return;
        }

        int current = _values.Count;
        if (length < current)
        {
            _values.RemoveRange(length, current - length);
            return;
        }

        while (_values.Count < length)
            _values.Add(null);
    }

    // ---- JSON converter (ignore computed properties like "length") ----
    internal sealed class ScriptArrayJsonConverter : JsonConverter<ScriptArray>
    {
        public override ScriptArray? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotSupportedException("Deserializing ScriptArray is not supported.");

        public override void Write(Utf8JsonWriter writer, ScriptArray value, JsonSerializerOptions options)
        {
            // Strategy: emit an object with numeric keys "0","1",... for array slots,
            // then append ScriptObject plain properties (merged view minus computed like "length").
            writer.WriteStartObject();

            // 1) numeric slots
            for (int i = 0; i < value._values.Count; i++)
            {
                writer.WritePropertyName(i.ToString(CultureInfo.InvariantCulture));
                JsonSerializer.Serialize(writer, value._values[i], options);
            }

            // 2) base properties (merged map) minus "length"
            //    (ToMergedDictionary() returns plain + computed; we filter out computed keys we know, i.e. "length")
            var map = value.ToMergedDictionary();
            foreach (var kv in map)
            {
                if (kv.Key == "length") continue; // ignore computed
                // skip if this overwrites a numeric index already emitted
                if (int.TryParse(kv.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idx) &&
                    idx >= 0 &&
                    idx < value._values.Count)
                    continue;

                writer.WritePropertyName(kv.Key);
                JsonSerializer.Serialize(writer, kv.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}