using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Parser.Structure.Action;

/// <summary>
/// Structure for the action record before decoding its payload.
/// </summary>
public sealed class ActionRecord
{
    /// <summary>Absolute byte offset of this action (within its block).</summary>
    public int Offset { get; }

    /// <summary>Action opcode.</summary>
    public Opcode Opcode { get; }

    /// <summary>Length of the action payload (0 for short/simple actions).</summary>
    public int Length { get; }

    /// <summary>Decoded payload (shape depends on opcode), or null.</summary>
    public object? Data { get; }

    public ActionRecord(int offset, Opcode opcode, int length, object? data)
    {
        Offset = offset;
        Opcode = opcode;
        Length = length;
        Data = data;
    }

    /// <summary>
    /// Read action records until the end of the current action block.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the block or somewhere inside it.</param>
    /// <param name="end">Absolute byte offset of the end of the block (exclusive).</param>
    public static List<ActionRecord> ReadCollection(SwfReader reader, int end)
    {
        if (reader.Offset >= end)
            return new List<ActionRecord>(0);

        if (end > reader.End)
        {
            if ((reader.Errors & Errors.OutOfBounds) != 0)
                throw ParserOutOfBoundException.CreateReadAfterEnd(end, reader.End);

            end = reader.End; // clamp
        }

        var actions = new List<ActionRecord>();

        // Create a bounded view for the block and fast-forward the main reader.
        var chunk = reader.Chunk(reader.Offset, end);
        reader.SkipTo(end);

        while (chunk.Offset < end)
        {
            var offset = chunk.Offset;
            var actionLength = 0;

            var actionCode = chunk.ReadUi8();
            if (actionCode == 0)
            {
                actions.Add(new ActionRecord(offset, Opcode.Null, 0, null));
                continue;
            }

            if (actionCode >= 0x80)
            {
                actionLength = chunk.ReadUi16(); // extended action with payload length
            }

            // Map byte -> Opcode
            bool ok = OpcodeExtensions.TryFrom(actionCode, out var opcode); // replace by Enum.IsDefined if preferred
            if (!ok)
            {
                if ((reader.Errors & Errors.InvalidData) != 0)
                {
                    throw new ParserInvalidDataException(
                        $"Invalid action code \"{actionCode}\" at offset {chunk.Offset}",
                        chunk.Offset
                    );
                }

                // ignore unknown action codes if INVALID_DATA not set
                continue;
            }

            object? actionData = actionLength > 0
                ? opcode.ReadData(chunk, actionLength) // extension method to implement per opcode
                : null;

            actions.Add(new ActionRecord(offset, opcode, actionLength, actionData));
        }

        return actions;
    }
}