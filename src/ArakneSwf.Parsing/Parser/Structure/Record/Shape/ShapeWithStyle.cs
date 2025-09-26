namespace ArakneSwf.Parsing.Parser.Structure.Record.Shape;

/// <summary>
/// Shape structure for DefineShapeTag / DefineShape4Tag.
/// </summary>
public sealed class ShapeWithStyle
{
    /// <summary>Liste des styles de remplissage.</summary>
    public IReadOnlyList<FillStyle> FillStyles { get; }

    /// <summary>Liste des styles de ligne.</summary>
    public IReadOnlyList<LineStyle> LineStyles { get; }

    /// <summary>Liste des enregistrements de forme (segments et changements de style).</summary>
    public IReadOnlyList<ShapeRecord> ShapeRecords { get; }

    public ShapeWithStyle(
        IReadOnlyList<FillStyle>   fillStyles,
        IReadOnlyList<LineStyle>   lineStyles,
        IReadOnlyList<ShapeRecord> shapeRecords)
    {
        FillStyles = fillStyles;
        LineStyles = lineStyles;
        ShapeRecords = shapeRecords;
    }

    /// <summary>
    /// Lit une structure ShapeWithStyle depuis le flux SWF.
    /// </summary>
    /// <param name="reader">Lecteur SWF.</param>
    /// <param name="version">Version du tag de forme (1..4).</param>
    public static ShapeWithStyle Read(SwfReader reader, int version)
    {
        var fillStyles   = FillStyle.ReadCollection(reader, version);
        var lineStyles   = LineStyle.ReadCollection(reader, version);
        var shapeRecords = ShapeRecord.ReadCollection(reader, version);

        return new ShapeWithStyle(fillStyles, lineStyles, shapeRecords);
    }
}
