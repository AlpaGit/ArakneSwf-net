namespace ArakneSwf.Parsing.Error;

/// <summary>
/// Erreur pour des données invalides/corrompues lors du parsing.
/// </summary>
[Serializable]
public sealed class ParserInvalidDataException : Exception
{
    /// <summary>Offset auquel l'erreur s'est produite.</summary>
    public int Position { get; }

    public ParserInvalidDataException(string message, int position)
        : this(message, position, innerException: null) { }

    public ParserInvalidDataException(string message, int position, Exception? innerException)
        : base(message, innerException)
    {
        Position = position;
    }

    /// <summary>
    /// Usine pour une erreur de données compressées invalides.
    /// </summary>
    public static ParserInvalidDataException CreateInvalidCompressedData(int position)
        => new ParserInvalidDataException("Invalid compressed data", position);
}