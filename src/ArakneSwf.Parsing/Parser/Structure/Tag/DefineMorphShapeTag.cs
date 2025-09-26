using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.MorphShape;
using ArakneSwf.Parsing.Parser.Structure.Record.Shape;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineMorphShape tag (TYPE = 46).
/// </summary>
public sealed class DefineMorphShapeTag
{
    public const int TYPE = 46;

    public int CharacterId { get; }
    public Rectangle StartBounds { get; }
    public Rectangle EndBounds { get; }

    /// <summary>
    /// Offset, in bytes, from the start of the morph data to the start of the end-shape data.
    /// </summary>
    public int Offset { get; }

    public IReadOnlyList<MorphFillStyle> FillStyles { get; }
    public IReadOnlyList<MorphLineStyle> LineStyles { get; }

    public IReadOnlyList<ShapeRecord> StartEdges { get; }
    public IReadOnlyList<ShapeRecord> EndEdges { get; }

    public DefineMorphShapeTag(
        int                           characterId,
        Rectangle                     startBounds,
        Rectangle                     endBounds,
        int                           offset,
        IReadOnlyList<MorphFillStyle> fillStyles,
        IReadOnlyList<MorphLineStyle> lineStyles,
        IReadOnlyList<ShapeRecord>    startEdges,
        IReadOnlyList<ShapeRecord>    endEdges)
    {
        CharacterId = characterId;
        StartBounds = startBounds;
        EndBounds = endBounds;
        Offset = offset;
        FillStyles = fillStyles;
        LineStyles = lineStyles;
        StartEdges = startEdges;
        EndEdges = endEdges;
    }

    /// <summary>
    /// Read a DefineMorphShape tag from the reader.
    /// </summary>
    public static DefineMorphShapeTag Read(SwfReader reader)
    {
        // The shape version only changes basic style records; morph shapes use dedicated morph styles.
        // It's therefore safe to parse shape records with version = 1 here.
        int characterId = reader.ReadUi16();
        var startBounds = Rectangle.Read(reader);
        var endBounds = Rectangle.Read(reader);
        var offset = (int)reader.ReadUi32();

        var fillStyles = MorphFillStyle.ReadCollection(reader);
        var lineStyles = MorphLineStyle.ReadCollection(reader);

        var startEdges = ShapeRecord.ReadCollection(reader, version: 1);
        var endEdges = ShapeRecord.ReadCollection(reader, version: 1);

        return new DefineMorphShapeTag(
            characterId,
            startBounds,
            endBounds,
            offset,
            fillStyles,
            lineStyles,
            startEdges,
            endEdges
        );
    }
}