namespace ArakneSwf.Parsing.Parser.Structure.Record.MorphShape;

/// <summary>
/// Style de ligne morph (largeurs et couleurs de début/fin).
/// </summary>
public sealed class MorphLineStyle
{
    public int StartWidth { get; }
    public int EndWidth   { get; }
    public Color StartColor { get; }
    public Color EndColor   { get; }

    public MorphLineStyle(int startWidth, int endWidth, Color startColor, Color endColor)
    {
        StartWidth = startWidth;
        EndWidth   = endWidth;
        StartColor = startColor;
        EndColor   = endColor;
    }

    /// <summary>
    /// Lit une collection de <see cref="MorphLineStyle"/>.
    /// Le nombre d'éléments est un UI8, ou 0xFF suivi d'un UI16 (compte étendu).
    /// </summary>
    public static List<MorphLineStyle> ReadCollection(SwfReader reader)
    {
        int count = reader.ReadUi8();
        if (count == 0xFF)
            count = reader.ReadUi16();

        var styles = new List<MorphLineStyle>(count);
        for (int i = 0; i < count; i++)
        {
            int startWidth = reader.ReadUi16();
            int endWidth   = reader.ReadUi16();
            var startColor = Color.ReadRgba(reader);
            var endColor   = Color.ReadRgba(reader);

            styles.Add(new MorphLineStyle(startWidth, endWidth, startColor, endColor));
        }

        return styles;
    }
}
