namespace ArakneSwf.Parsing.Parser.Structure.Record.Filters;

/// <summary>
/// Filtre "ColorMatrix" (matrice 4x5 = 20 valeurs).
/// </summary>
public sealed class ColorMatrixFilter : Filter
{
    public const byte FILTER_ID = 6;

    private readonly float[] _matrix; // immuable après construction

    /// <summary>
    /// Matrice (taille exacte : 20).
    /// Ordre identique au flux SWF.
    /// </summary>
    public IReadOnlyList<float> Matrix => _matrix;

    public ColorMatrixFilter(IReadOnlyList<float> matrix)
    {
        if (matrix is null) throw new ArgumentNullException(nameof(matrix));
        if (matrix.Count != 20)
            throw new ArgumentException("Matrix size must be exactly 20.", nameof(matrix));

        _matrix = new float[20];
        for (var i = 0; i < 20; i++) _matrix[i] = matrix[i];
    }

    /// <summary>
    /// Lit un <see cref="ColorMatrixFilter"/> depuis le flux SWF.
    /// </summary>
    public static ColorMatrixFilter Read(SwfReader reader)
    {
        var m = new float[20];
        for (var i = 0; i < 20; i++)
            m[i] = reader.ReadFloat();

        return new ColorMatrixFilter(m);
    }
}