using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Parser.Structure.Action;

/// <summary>
/// Stocke une valeur primitive ActionScript avec son type.
/// </summary>
public sealed class Value
{
    /// <summary>Type de la valeur (voir <see cref="ValueType"/>).</summary>
    public ValueType Type { get; }

    /// <summary>Valeur parsée (string, float, double, int, byte, ushort, bool, ou null).</summary>
    public object? Data { get; }

    public Value(ValueType type, object? data)
    {
        Type = type;
        Data = data;
    }

    /// <summary>
    /// Lit une collection de valeurs jusqu’à avoir consommé <paramref name="length"/> octets.
    /// </summary>
    /// <param name="reader">Lecteur SWF.</param>
    /// <param name="length">Longueur de la collection, en octets.</param>
    public static List<Value> ReadCollection(SwfReader reader, int length)
    {
        var values = new List<Value>();
        var bytePosEnd = reader.Offset + length;

        if (bytePosEnd > reader.End)
        {
            if ((reader.Errors & Errors.OutOfBounds) != 0)
                throw ParserOutOfBoundException.CreateReadTooManyBytes(reader.Offset, reader.End, length);

            bytePosEnd = reader.End; // clamp
        }

        while (reader.Offset < bytePosEnd)
        {
            var typeId = reader.ReadUi8();
            if (!ValueTypeExtensions.TryFrom(typeId, out var type))
            {
                if ((reader.Errors & Errors.InvalidData) != 0)
                    throw new ParserInvalidDataException(
                        $"Invalid value type \"{typeId}\" at offset {reader.Offset}",
                        reader.Offset
                    );

                // sinon ignorer et continuer
                continue;
            }

            object? data = type switch
            {
                ValueType.String => reader.ReadNullTerminatedString(),
                ValueType.Float => reader.ReadFloat(), // float (32 bits)
                ValueType.Null => null,
                ValueType.Undefined => null,
                ValueType.Register => reader.ReadUi8(), // byte
                ValueType.Boolean => reader.ReadUi8() == 1, // strictement 1 => true
                ValueType.Double => reader.ReadDouble(), // double (64 bits)
                ValueType.Integer => reader.ReadSi32(), // int32 signé
                ValueType.Constant8 => reader.ReadUi8(), // byte (id constant pool)
                ValueType.Constant16 => reader.ReadUi16(), // ushort (id constant pool)
                _ => throw new ParserInvalidDataException($"Unhandled value type {type}", reader.Offset)
            };

            values.Add(new Value(type, data));
        }

        return values;
    }
}

/// <summary>Extensions utilitaires pour <see cref="ValueType"/>.</summary>
public static class ValueTypeExtensions
{
    public static bool TryFrom(byte id, out ValueType type)
    {
        if (Enum.IsDefined(typeof(ValueType), (int)id))
        {
            type = (ValueType)id;
            return true;
        }

        type = default;
        return false;
    }
}