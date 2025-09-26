using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure;

/// <summary>
/// En-tête SWF.
/// </summary>
/// <param name="Signature">"FWS" ou "CWS".</param>
/// <param name="Version">Version du fichier SWF (>= 0).</param>
/// <param name="FileLength">
/// Longueur totale du fichier en octets (UI32). Pour "CWS", longueur de la version décompressée.
/// </param>
/// <param name="FrameSize">Taille de la scène en twips.</param>
/// <param name="FrameRate">Fréquence d’image.</param>
/// <param name="FrameCount">Nombre total d’images (>= 0).</param>
public sealed record SwfHeader(
    string    Signature,
    int       Version,
    uint      FileLength,
    Rectangle FrameSize,
    float     FrameRate,
    int       FrameCount
);
