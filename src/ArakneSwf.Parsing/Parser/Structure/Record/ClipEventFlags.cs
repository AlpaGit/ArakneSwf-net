namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Bitfield wrapper for clip event flags.
/// Flags are 16 bits for SWF ≤ 5, and 32 bits for SWF ≥ 6.
/// </summary>
public sealed class ClipEventFlags
{
    // First byte
    public const int KEY_UP = 0x80;
    public const int KEY_DOWN = 0x40;
    public const int MOUSE_UP = 0x20;
    public const int MOUSE_DOWN = 0x10;
    public const int MOUSE_MOVE = 0x08;
    public const int UNLOAD = 0x04;
    public const int ENTER_FRAME = 0x02;
    public const int LOAD = 0x01;

    // Second byte
    public const int DRAG_OVER = 0x8000;
    public const int ROLL_OUT = 0x4000;
    public const int ROLL_OVER = 0x2000;
    public const int RELEASE_OUTSIDE = 0x1000;
    public const int RELEASE = 0x0800;
    public const int PRESS = 0x0400;
    public const int INITIALIZE = 0x0200;
    public const int DATA = 0x0100;

    // Third byte (SWF ≥ 6)
    public const int CONSTRUCT = 0x040000;
    public const int KEY_PRESS = 0x020000;
    public const int DRAG_OUT = 0x010000;

    /// <summary>
    /// Raw flags value (16 or 32 bits depending on SWF version).
    /// </summary>
    public int Flags { get; }

    public ClipEventFlags(int flags) => Flags = flags;

    /// <summary>Returns true if the given flag (one of the constants) is set.</summary>
    public bool Has(int flag) => (Flags & flag) == flag;

    /// <summary>
    /// Reads flags from the stream (UI16 for SWF ≤ 5, UI32 for SWF ≥ 6).
    /// </summary>
    public static ClipEventFlags Read(SwfReader reader, int version)
    {
        var flags = version <= 5 ? reader.ReadUi16() : reader.ReadUi32();
        return new ClipEventFlags((int)flags);
    }
}