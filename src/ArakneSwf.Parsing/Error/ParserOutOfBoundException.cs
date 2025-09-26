namespace ArakneSwf.Parsing.Error;

/// <summary>
/// Erreur levée lors d'une tentative de lecture au-delà des limites du flux binaire.
/// </summary>
[Serializable]
public sealed class ParserOutOfBoundException : Exception
{
    /// <summary>Offset auquel l'erreur s'est produite.</summary>
    public int Position { get; }

    /// <summary>Fin de la zone lisible (exclusive), si connue.</summary>
    public int? End { get; }

    /// <summary>Nombre d'octets demandés (si applicable).</summary>
    public int? RequestedBytes { get; }

    public ParserOutOfBoundException(string message, int position)
        : this(message, position, end: null, requestedBytes: null, innerException: null) { }

    public ParserOutOfBoundException(string     message, int position, int? end, int? requestedBytes = null,
                                     Exception? innerException = null)
        : base(message, innerException)
    {
        Position = position;
        End = end;
        RequestedBytes = requestedBytes;
    }
    
    /// <summary>
    /// Tentative de lecture à un offset &gt;= fin.
    /// </summary>
    public static ParserOutOfBoundException CreateReadAfterEnd(int position, int end)
    {
        var msg = $"Trying to read after the end of the stream: offset {position} >= end {end}";
        return new ParserOutOfBoundException(msg, position, end);
    }

    /// <summary>
    /// Tentative de lecture de <paramref name="requestedBytes"/> octet(s) alors que la fin est atteinte.
    /// </summary>
    public static ParserOutOfBoundException CreateReadTooManyBytes(int position, int end, int requestedBytes)
    {
        var msg = $"Trying to read {requestedBytes} byte(s) at offset {position}, but end is {end}";
        return new ParserOutOfBoundException(msg, position, end, requestedBytes);
    }

}