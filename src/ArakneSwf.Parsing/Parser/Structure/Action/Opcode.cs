using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Parser.Structure.Action;

/// <summary>
/// Enum of all ActionScript 2 bytecodes (opcode values).
/// </summary>
public enum Opcode : byte
{
    Null = 0x00,

    // SWF 3
    ActionGotoFrame = 0x81,
    ActionGetUrl = 0x83,
    ActionNextFrame = 0x04,
    ActionPreviousFrame = 0x05,
    ActionPlay = 0x06,
    ActionStop = 0x07,
    ActionToggleQuality = 0x08,
    ActionStopSounds = 0x09,
    ActionWaitForFrame = 0x8A,
    ActionSetTarget = 0x8B,
    ActionGoToLabel = 0x8C,

    // SWF 4
    ActionPush = 0x96, // Stack operations
    ActionPop = 0x17,
    ActionAdd = 0x0A, // Arithmetic operators
    ActionSubtract = 0x0B,
    ActionMultiply = 0x0C,
    ActionDivide = 0x0D,
    ActionEquals = 0x0E, // Numerical comparison
    ActionLess = 0x0F,
    ActionAnd = 0x10, // Logical operators
    ActionOr = 0x11,
    ActionNot = 0x12,
    ActionStringEquals = 0x13, // String manipulation
    ActionStringLength = 0x14,
    ActionStringAdd = 0x21,
    ActionStringExtract = 0x15,
    ActionStringLess = 0x29,
    ActionMbStringLength = 0x31,
    ActionMbStringExtract = 0x35,
    ActionToInteger = 0x18, // Type conversion
    ActionCharToAscii = 0x32,
    ActionAsciiToChar = 0x33,
    ActionMbCharToAscii = 0x36,
    ActionMbAsciiToChar = 0x37,
    ActionJump = 0x99, // Control flow
    ActionIf = 0x9D,
    ActionCall = 0x9E,
    ActionGetVariable = 0x1C, // Variables
    ActionSetVariable = 0x1D,
    ActionGetUrl2 = 0x9A, // Movie control
    ActionGotoFrame2 = 0x9F,
    ActionSetTarget2 = 0x20,
    ActionGetProperty = 0x22,
    ActionSetProperty = 0x23,
    ActionCloneSprite = 0x24,
    ActionRemoteSprite = 0x25,
    ActionStartDrag = 0x27,
    ActionEndDrag = 0x28,
    ActionWaitForFrame2 = 0x8D,
    ActionTrace = 0x26, // Utilities
    ActionGetTime = 0x34,
    ActionRandomNumber = 0x30,

    // SWF 5
    ActionCallFunction = 0x3D, // ScriptObject actions
    ActionCallMethod = 0x52,
    ActionConstantPool = 0x88,
    ActionDefineFunction = 0x9B,
    ActionDefineLocal = 0x3C,
    ActionDefineLocal2 = 0x41,
    ActionDelete = 0x3A,
    ActionDelete2 = 0x3B,
    ActionEnumerate = 0x46,
    ActionEquals2 = 0x49,
    ActionGetMember = 0x4E,
    ActionInitArray = 0x42,
    ActionInitObject = 0x43,
    ActionNewMethod = 0x53,
    ActionNewObject = 0x40,
    ActionSetMember = 0x4F,
    ActionTargetPath = 0x45,
    ActionWith = 0x94,
    ActionToNumber = 0x4A, // Type actions
    ActionToString = 0x4B,
    ActionTypeOf = 0x44,
    ActionAdd2 = 0x47, // Math actions
    ActionLess2 = 0x48,
    ActionModule = 0x3F,
    ActionBitAnd = 0x60, // Stack operator actions
    ActionBitLShift = 0x63,
    ActionBitOr = 0x61,
    ActionBitRShift = 0x64,
    ActionBitUrShift = 0x65,
    ActionBitXor = 0x62,
    ActionDecrement = 0x51,
    ActionIncrement = 0x50,
    ActionPushDuplicate = 0x4C,
    ActionReturn = 0x3E,
    ActionStackSwap = 0x4D,
    ActionStoreRegister = 0x87,

    // SWF 6
    DoInitAction = 0x59,
    ActionInstanceOf = 0x54,
    ActionEnumerate2 = 0x55,
    ActionStrictEquals = 0x66,
    ActionGreater = 0x67,
    ActionStringGreater = 0x68,

    // SWF 7
    ActionDefineFunction2 = 0x8E,
    ActionExtends = 0x69,
    ActionCastOp = 0x2B,
    ActionImplementsOp = 0x2C,
    ActionTry = 0x8F,
    ActionThrow = 0x2A,

    // SWF 9
    DoAbc = 0x82,
    // SWF 10 (no entries added here in this list)
}

public static class OpcodeExtensions
{
    /// <summary>Try to map a byte to an <see cref="Opcode"/> value.</summary>
    public static bool TryFrom(byte code, out Opcode opcode)
    {
        if (Enum.IsDefined(typeof(Opcode), (int)code))
        {
            opcode = (Opcode)code;
            return true;
        }

        opcode = default;
        return false;
    }

    /// <summary>
    /// Read the payload data of the action related to the opcode (PHP: <c>readData</c>).
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="length">Payload length as specified in the action record.</param>
    /// <param name="opcode"></param>
    public static object? ReadData(this Opcode opcode, SwfReader reader, int length)
    {
        switch (opcode)
        {
            case Opcode.ActionGotoFrame:
                return reader.ReadUi16();

            case Opcode.ActionGetUrl:
                return GetUrlData.Read(reader);

            case Opcode.ActionStoreRegister:
                return reader.ReadUi8();

            case Opcode.ActionConstantPool:
                return ReadConstantPool(reader);

            case Opcode.ActionWaitForFrame:
                return WaitForFrameData.Read(reader);

            case Opcode.ActionSetTarget:
            case Opcode.ActionGoToLabel:
                return reader.ReadNullTerminatedString();

            case Opcode.ActionWaitForFrame2:
                return reader.ReadUi8();

            case Opcode.ActionDefineFunction2:
                return DefineFunction2Data.Read(reader);

            case Opcode.ActionWith:
            {
                int size = reader.ReadUi16();
                return reader.ReadBytes(size);
            }

            case Opcode.ActionPush:
                // Parsing ActionPush values exhaustively is complex; if you already have a Value parser,
                // replace this with it. Here we return raw bytes of the payload for simplicity.
                return reader.ReadBytes(length);

            case Opcode.ActionJump:
            case Opcode.ActionIf:
                return reader.ReadSi16();

            case Opcode.ActionGetUrl2:
                return GetUrl2Data.Read(reader);

            case Opcode.ActionDefineFunction:
                return DefineFunctionData.Read(reader);

            case Opcode.ActionGotoFrame2:
                return GotoFrame2Data.Read(reader);

            default:
                if ((reader.Errors & Errors.InvalidData) != 0)
                    throw new ParserInvalidDataException(
                        $"Unexpected data for opcode {opcode}, actionLength={length}",
                        reader.Offset
                    );

                return reader.ReadBytes(length);
        }
    }
    



    /// <summary>Read ConstantPool strings (PHP: <c>readConstantPool</c>).</summary>
    private static List<string> ReadConstantPool(SwfReader reader)
    {
        var list = new List<string>();
        int count = reader.ReadUi16();
        for (int i = 0; i < count; i++)
            list.Add(reader.ReadNullTerminatedString());
        return list;
    }
}
