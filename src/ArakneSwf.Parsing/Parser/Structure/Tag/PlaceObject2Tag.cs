using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// PlaceObject2 tag (TYPE = 26).
/// </summary>
public sealed class PlaceObject2Tag
{
    public const int TYPE = 26;

    public bool Move { get; }
    public int Depth { get; }
    public int? CharacterId { get; }
    public Matrix? Matrix { get; }
    public ColorTransform? ColorTransform { get; }
    public int? Ratio { get; }
    public string? Name { get; }
    public int? ClipDepth { get; }
    public ClipActions? ClipActions { get; }

    public PlaceObject2Tag(
        bool            move,
        int             depth,
        int?            characterId,
        Matrix?         matrix,
        ColorTransform? colorTransform,
        int?            ratio,
        string?         name,
        int?            clipDepth,
        ClipActions?    clipActions)
    {
        Move = move;
        Depth = depth;
        CharacterId = characterId;
        Matrix = matrix;
        ColorTransform = colorTransform;
        Ratio = ratio;
        Name = name;
        ClipDepth = clipDepth;
        ClipActions = clipActions;
    }

    /// <summary>
    /// Read a PlaceObject2 tag.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="swfVersion">SWF version of the file being read.</param>
    public static PlaceObject2Tag Read(SwfReader reader, int swfVersion)
    {
        int flags = reader.ReadUi8();
        var hasClipActions = (flags & 0b1000_0000) != 0;
        var hasClipDepth = (flags & 0b0100_0000) != 0;
        var hasName = (flags & 0b0010_0000) != 0;
        var hasRatio = (flags & 0b0001_0000) != 0;
        var hasColorTransform = (flags & 0b0000_1000) != 0;
        var hasMatrix = (flags & 0b0000_0100) != 0;
        var hasCharacter = (flags & 0b0000_0010) != 0;
        var move = (flags & 0b0000_0001) != 0;

        int depth = reader.ReadUi16();
        var characterId = hasCharacter ? reader.ReadUi16() : (int?)null;
        var matrix = hasMatrix ? Matrix.Read(reader) : null;
        var cxform = hasColorTransform ? ColorTransform.Read(reader, withAlpha: true) : null;
        var ratio = hasRatio ? reader.ReadUi16() : (int?)null;
        var name = hasName ? reader.ReadNullTerminatedString() : null;
        var clipDepth = hasClipDepth ? reader.ReadUi16() : (int?)null;
        var clipActions = hasClipActions ? ClipActions.Read(reader, swfVersion) : null;

        return new PlaceObject2Tag(
            move: move,
            depth: depth,
            characterId: characterId,
            matrix: matrix,
            colorTransform: cxform,
            ratio: ratio,
            name: name,
            clipDepth: clipDepth,
            clipActions: clipActions
        );
    }
}