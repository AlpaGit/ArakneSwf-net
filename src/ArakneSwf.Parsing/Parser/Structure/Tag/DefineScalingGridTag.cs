using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineScalingGrid tag (TYPE = 78).
/// </summary>
public sealed class DefineScalingGridTag
{
    public const int TYPE = 78;

    public int CharacterId { get; }
    public Rectangle Splitter { get; }

    public DefineScalingGridTag(int characterId, Rectangle splitter)
    {
        CharacterId = characterId;
        Splitter = splitter;
    }

    /// <summary>
    /// Read a DefineScalingGrid tag from the SWF reader.
    /// </summary>
    public static DefineScalingGridTag Read(SwfReader reader)
    {
        int characterId = reader.ReadUi16();
        var splitter = Rectangle.Read(reader);

        return new DefineScalingGridTag(characterId, splitter);
    }
}