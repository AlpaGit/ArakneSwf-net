namespace ArakneSwf.Parsing.Parser.Structure.Action;

public sealed record WaitForFrameData(ushort Frame, byte SkipCount)
{
    public static WaitForFrameData Read(SwfReader r)
        => new WaitForFrameData(r.ReadUi16(), r.ReadUi8());
}