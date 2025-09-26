namespace ArakneSwf.Parsing.Error;

/// <summary>
/// Erreur levée lorsqu'une quantité de données supérieure au maximum attendu est détectée.
/// </summary>
[Serializable]
public sealed class ParserExtraDataException : Exception
{
    /// <summary>Offset auquel l'erreur s'est produite.</summary>
    public int Position { get; }

    /// <summary>Longueur maximale autorisée (si connue).</summary>
    public int? MaxAllowedLength { get; }

    /// <summary>Longueur réellement observée (si connue).</summary>
    public long? ActualLength { get; }

    public ParserExtraDataException(string message, int position, int? maxAllowedLength = null, long? actualLength = null)
        : this(message, position, maxAllowedLength, actualLength, innerException: null) { }

    public ParserExtraDataException(string message, int position, int? maxAllowedLength, long? actualLength, Exception? innerException)
        : base(message, innerException)
    {
        Position = position;
        MaxAllowedLength = maxAllowedLength;
        ActualLength = actualLength;
    }

}