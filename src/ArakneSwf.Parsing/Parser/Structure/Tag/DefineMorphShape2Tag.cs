using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.MorphShape;
using ArakneSwf.Parsing.Parser.Structure.Record.Shape;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineMorphShape2 tag (TYPE = 84).
/// </summary>
public sealed class DefineMorphShape2Tag
{
    public const int TYPE = 84;

    public int CharacterId { get; }
    public Rectangle StartBounds { get; }
    public Rectangle EndBounds { get; }
    public Rectangle StartEdgeBounds { get; }
    public Rectangle EndEdgeBounds { get; }

    public bool UsesNonScalingStrokes { get; }
    public bool UsesScalingStrokes { get; }

    /// <summary>
    /// Offset, in bytes, from the start of this tag's morph data to the start of the end-shape data.
    /// </summary>
    public int Offset { get; }

    public IReadOnlyList<MorphFillStyle> FillStyles { get; }
    public IReadOnlyList<MorphLineStyle2> LineStyles { get; }

    public IReadOnlyList<ShapeRecord> StartEdges { get; }
    public IReadOnlyList<ShapeRecord> EndEdges { get; }

    public DefineMorphShape2Tag(
        int                            characterId,
        Rectangle                      startBounds,
        Rectangle                      endBounds,
        Rectangle                      startEdgeBounds,
        Rectangle                      endEdgeBounds,
        bool                           usesNonScalingStrokes,
        bool                           usesScalingStrokes,
        int                            offset,
        IReadOnlyList<MorphFillStyle>  fillStyles,
        IReadOnlyList<MorphLineStyle2> lineStyles,
        IReadOnlyList<ShapeRecord>     startEdges,
        IReadOnlyList<ShapeRecord>     endEdges)
    {
        CharacterId = characterId;
        StartBounds = startBounds;
        EndBounds = endBounds;
        StartEdgeBounds = startEdgeBounds;
        EndEdgeBounds = endEdgeBounds;
        UsesNonScalingStrokes = usesNonScalingStrokes;
        UsesScalingStrokes = usesScalingStrokes;
        Offset = offset;
        FillStyles = fillStyles;
        LineStyles = lineStyles;
        StartEdges = startEdges;
        EndEdges = endEdges;
    }

    /// <summary>
    /// Read a DefineMorphShape2 tag from the stream.
    /// </summary>
    public static DefineMorphShape2Tag Read(SwfReader reader)
    {
        int characterId = reader.ReadUi16();
        var startBounds = Rectangle.Read(reader);
        var endBounds = Rectangle.Read(reader);

        var startEdgeBounds = Rectangle.Read(reader);
        var endEdgeBounds = Rectangle.Read(reader);

        var flags = reader.ReadUi8();
        // top 6 bits reserved
        var usesNonScalingStrokes = (flags & 0b0000_0010) != 0;
        var usesScalingStrokes = (flags & 0b0000_0001) != 0;

        // Note: morph shapes use dedicated morph styles, so shape version differences
        // only affect style records; we can safely pass version 1 to ShapeRecord here.
        var offset = (int)reader.ReadUi32();

        var fillStyles = MorphFillStyle.ReadCollection(reader);
        var lineStyles = MorphLineStyle2.ReadCollection(reader);

        var startEdges = ShapeRecord.ReadCollection(reader, version: 1);
        var endEdges = ShapeRecord.ReadCollection(reader, version: 1);

        return new DefineMorphShape2Tag(
            characterId,
            startBounds,
            endBounds,
            startEdgeBounds,
            endEdgeBounds,
            usesNonScalingStrokes,
            usesScalingStrokes,
            offset,
            fillStyles,
            lineStyles,
            startEdges,
            endEdges
        );
    }
}