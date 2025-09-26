namespace ArakneSwf.Parsing.Parser.Structure.Record.Shape;

/// <summary>
/// Enregistrement de changement de style dans un tracé SWF.
/// </summary>
public sealed class StyleChangeRecord : ShapeRecord
{
    public bool StateNewStyles { get; }
    public bool StateLineStyle { get; }
    public bool StateFillStyle0 { get; }
    public bool StateFillStyle1 { get; }
    public bool StateMoveTo { get; }

    public int MoveDeltaX { get; }
    public int MoveDeltaY { get; }

    public int FillStyle0 { get; }
    public int FillStyle1 { get; }
    public int LineStyle { get; }

    /// <summary>Nouveaux styles de remplissage si <see cref="StateNewStyles"/> est vrai.</summary>
    public IReadOnlyList<FillStyle> FillStyles { get; }

    /// <summary>Nouveaux styles de ligne si <see cref="StateNewStyles"/> est vrai.</summary>
    public IReadOnlyList<LineStyle> LineStyles { get; }

    public StyleChangeRecord(
        bool                     stateNewStyles,
        bool                     stateLineStyle,
        bool                     stateFillStyle0,
        bool                     stateFillStyle1,
        bool                     stateMoveTo,
        int                      moveDeltaX,
        int                      moveDeltaY,
        int                      fillStyle0,
        int                      fillStyle1,
        int                      lineStyle,
        IReadOnlyList<FillStyle> fillStyles,
        IReadOnlyList<LineStyle> lineStyles)
    {
        StateNewStyles = stateNewStyles;
        StateLineStyle = stateLineStyle;
        StateFillStyle0 = stateFillStyle0;
        StateFillStyle1 = stateFillStyle1;
        StateMoveTo = stateMoveTo;

        MoveDeltaX = moveDeltaX;
        MoveDeltaY = moveDeltaY;

        FillStyle0 = fillStyle0;
        FillStyle1 = fillStyle1;
        LineStyle = lineStyle;

        FillStyles = fillStyles ?? new List<FillStyle>(0);
        LineStyles = lineStyles ?? new List<LineStyle>(0);
    }

    /// <summary>
    /// Indique si un reset complet du contexte de dessin est demandé.
    /// Si vrai, les tracés suivants doivent être traités comme une nouvelle forme.
    /// </summary>
    public bool Reset()
        => StateNewStyles && StateLineStyle && StateFillStyle0 && StateFillStyle1 && StateMoveTo;
}