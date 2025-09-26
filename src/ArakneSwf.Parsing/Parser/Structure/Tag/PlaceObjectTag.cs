using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// PlaceObject (TYPE = 4)
/// </summary>
public sealed class PlaceObjectTag
{
    public const int TYPE = 4;

    public int CharacterId { get; }
    public int Depth { get; }
    public Matrix Matrix { get; }
    public ColorTransform? ColorTransform { get; }

    public PlaceObjectTag(
        int             characterId,
        int             depth,
        Matrix          matrix,
        ColorTransform? colorTransform)
    {
        CharacterId = characterId;
        Depth = depth;
        Matrix = matrix;
        ColorTransform = colorTransform;
    }

    /// <summary>
    /// Read a PlaceObject tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset for this tag (exclusive).</param>
    public static PlaceObjectTag Read(SwfReader reader, int end)
    {
        int characterId = reader.ReadUi16();
        int depth = reader.ReadUi16();
        var matrix = Matrix.Read(reader);

        // Optional ColorTransform (no alpha) if there's remaining data in this tag.
        var cx = reader.Offset < end
            ? ColorTransform.Read(reader, withAlpha: false)
            : null;

        return new PlaceObjectTag(characterId, depth, matrix, cx);
    }
}