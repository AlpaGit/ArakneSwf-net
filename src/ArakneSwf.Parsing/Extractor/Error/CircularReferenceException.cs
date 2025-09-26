using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Extractor.Error;

/// <summary>
/// Erreur de référence circulaire lors du traitement d’un caractère SWF.
/// </summary>
public sealed class CircularReferenceException : Exception, IExtractorException
{
    /// <summary>Identifiant du caractère impliqué.</summary>
    public int CharacterId { get; }

    public CircularReferenceException(string message, int characterId)
        : base(message)
    {
        CharacterId = characterId;
        HResult = (int)Errors.CircularReference;
    }
}