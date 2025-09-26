namespace ArakneSwf.Parsing.Extractor.Timelines;

public enum BlendMode : int
{
    Normal = 1,
    Layer = 2,
    Multiply = 3,
    Screen = 4,
    Lighten = 5,
    Darken = 6,
    Difference = 7,
    Add = 8,
    Subtract = 9,
    Invert = 10,
    Alpha = 11,
    Erase = 12,
    Overlay = 13,
    Hardlight = 14
}

public static class BlendModeExtensions
{
    /// <summary>
    /// Renvoie la valeur CSS "mix-blend-mode" correspondante, ou null si non supportée / par défaut.
    /// </summary>
    public static string? ToCssValue(this BlendMode mode) => mode switch
    {
        BlendMode.Multiply                     => "multiply",
        BlendMode.Screen                       => "screen",
        BlendMode.Lighten or BlendMode.Add     => "lighten",
        BlendMode.Darken or BlendMode.Subtract => "darken",
        BlendMode.Difference                   => "difference",
        BlendMode.Overlay                      => "overlay",
        BlendMode.Hardlight                    => "hard-light",
        _                                      => null,
    };
}
