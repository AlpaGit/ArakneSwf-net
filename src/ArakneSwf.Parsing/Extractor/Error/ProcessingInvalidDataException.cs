using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Extractor.Error;

/// <summary>
/// Données impossibles à traiter (incohérentes ou invalides) pendant l’extraction.
/// </summary>
public sealed class ProcessingInvalidDataException : Exception, IExtractorException
{
    public ProcessingInvalidDataException(string? message = null, Exception? innerException = null)
        : base(message ?? string.Empty, innerException)
    {
        HResult = (int)Errors.UnprocessableData;
    }
}
