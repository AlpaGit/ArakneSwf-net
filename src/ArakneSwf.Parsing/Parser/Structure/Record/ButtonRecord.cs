using ArakneSwf.Parsing.Parser.Structure.Record.Filters;

namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// Enregistrement de bouton (ButtonRecord) pour DefineButton / DefineButton2.
/// </summary>
public sealed class ButtonRecord
{
    public bool StateHitTest { get; }
    public bool StateDown { get; }
    public bool StateOver { get; }
    public bool StateUp { get; }

    public int CharacterId { get; }
    public int PlaceDepth { get; }

    public Matrix Matrix { get; }
    public ColorTransform? ColorTransform { get; }

    /// <summary>Filtres appliqués (optionnels, DefineButton2).</summary>
    public IReadOnlyList<Filter>? Filters { get; }

    /// <summary>Mode de fusion (optionnel, DefineButton2).</summary>
    public int? BlendMode { get; }

    public ButtonRecord(
        bool                   stateHitTest,
        bool                   stateDown,
        bool                   stateOver,
        bool                   stateUp,
        int                    characterId,
        int                    placeDepth,
        Matrix                 matrix,
        ColorTransform?        colorTransform = null,
        IReadOnlyList<Filter>? filters        = null,
        int?                   blendMode      = null)
    {
        StateHitTest = stateHitTest;
        StateDown = stateDown;
        StateOver = stateOver;
        StateUp = stateUp;

        CharacterId = characterId;
        PlaceDepth = placeDepth;

        Matrix = matrix;
        ColorTransform = colorTransform;
        Filters = filters;
        BlendMode = blendMode;
    }

    /// <summary>
    /// Lit une collection de <see cref="ButtonRecord"/>.
    /// La fin de la collection est marquée par un octet de flags égal à 0.
    /// </summary>
    /// <param name="reader">Lecteur SWF.</param>
    /// <param name="version">Version du tag (1 pour DefineButton, ≥2 pour DefineButton2).</param>
    public static List<ButtonRecord> ReadCollection(SwfReader reader, int version)
    {
        var records = new List<ButtonRecord>();

        while (reader.Offset < reader.End)
        {
            byte flags = reader.ReadUi8();
            if (flags == 0)
                break;

            // bits 7..6 réservés
            bool hasBlendMode = (flags & 0b0010_0000) != 0;
            bool hasFilters = (flags & 0b0001_0000) != 0;
            bool stateHitTest = (flags & 0b0000_1000) != 0;
            bool stateDown = (flags & 0b0000_0100) != 0;
            bool stateOver = (flags & 0b0000_0010) != 0;
            bool stateUp = (flags & 0b0000_0001) != 0;

            int characterId = reader.ReadUi16();
            int placeDepth = reader.ReadUi16();
            var matrix = Matrix.Read(reader);

            ColorTransform? colorTransform = (version >= 2) ? ColorTransform.Read(reader, withAlpha: true) : null;
            List<Filter>? filters = (version >= 2 && hasFilters) ? Filter.ReadCollection(reader) : null;
            int? blendMode = (version >= 2 && hasBlendMode) ? reader.ReadUi8() : (int?)null;

            records.Add(new ButtonRecord(
                            stateHitTest: stateHitTest,
                            stateDown: stateDown,
                            stateOver: stateOver,
                            stateUp: stateUp,
                            characterId: characterId,
                            placeDepth: placeDepth,
                            matrix: matrix,
                            colorTransform: colorTransform,
                            filters: filters,
                            blendMode: blendMode
                        ));
        }

        return records;
    }
}