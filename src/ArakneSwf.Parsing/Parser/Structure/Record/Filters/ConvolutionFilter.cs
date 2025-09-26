namespace ArakneSwf.Parsing.Parser.Structure.Record.Filters;

/// <summary>
/// Filtre de convolution.
/// </summary>
public sealed class ConvolutionFilter : Filter
{
    public const byte FILTER_ID = 5;

    public int MatrixX { get; }
    public int MatrixY { get; }
    public float Divisor { get; }
    public float Bias { get; }

    /// <summary>Matrice de convolution (taille = MatrixX * MatrixY).</summary>
    public IReadOnlyList<float> Matrix { get; }

    public Color DefaultColor { get; }
    public bool Clamp { get; }
    public bool PreserveAlpha { get; }

    public ConvolutionFilter(
        int                  matrixX,
        int                  matrixY,
        float                divisor,
        float                bias,
        IReadOnlyList<float> matrix,
        Color                defaultColor,
        bool                 clamp,
        bool                 preserveAlpha)
    {
        if (matrix is null) throw new ArgumentNullException(nameof(matrix));
        if (matrixX <= 0 || matrixY <= 0)
            throw new ArgumentOutOfRangeException(nameof(matrixX), "Matrix dimensions must be positive.");
        if (matrix.Count != matrixX * matrixY)
            throw new ArgumentException("Matrix size must be matrixX * matrixY.", nameof(matrix));

        MatrixX = matrixX;
        MatrixY = matrixY;
        Divisor = divisor;
        Bias = bias;
        Matrix = matrix;
        DefaultColor = defaultColor ?? throw new ArgumentNullException(nameof(defaultColor));
        Clamp = clamp;
        PreserveAlpha = preserveAlpha;
    }

    /// <summary>
    /// Lit un <see cref="ConvolutionFilter"/> depuis le flux SWF.
    /// </summary>
    public static ConvolutionFilter Read(SwfReader reader)
    {
        int matrixX = reader.ReadUi8();
        int matrixY = reader.ReadUi8();
        var divisor = reader.ReadFloat();
        var bias = reader.ReadFloat();

        var count = matrixX * matrixY;
        var matrix = new float[count];
        for (var i = 0; i < count; i++)
            matrix[i] = reader.ReadFloat();

        var defaultColor = Color.ReadRgba(reader);

        var flags = reader.ReadUi8();
        // 6 bits réservés (bits 7..2)
        var clamp = (flags & 0b0000_0010) != 0;         // bit 1
        var preserveAlpha = (flags & 0b0000_0001) != 0; // bit 0

        return new ConvolutionFilter(
            matrixX: matrixX,
            matrixY: matrixY,
            divisor: divisor,
            bias: bias,
            matrix: matrix,
            defaultColor: defaultColor,
            clamp: clamp,
            preserveAlpha: preserveAlpha
        );
    }
}