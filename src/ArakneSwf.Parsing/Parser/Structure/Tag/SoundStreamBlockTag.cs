namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// SoundStreamBlock (TYPE = 19)
/// </summary>
public sealed class SoundStreamBlockTag
{
    public const int TYPE = 19;

    /// <summary>
    /// Données audio brutes du bloc.
    /// </summary>
    public byte[] SoundData { get; }

    public SoundStreamBlockTag(byte[] soundData)
    {
        SoundData = soundData;
    }

    /// <summary>
    /// Lit un tag SoundStreamBlock depuis le lecteur SWF.
    /// </summary>
    /// <param name="reader">Lecteur binaire SWF.</param>
    /// <param name="end">Offset de fin du tag (exclusif).</param>
    /// <returns>Instance de <see cref="SoundStreamBlockTag"/>.</returns>
    /// <exception cref="ParserOutOfBoundException">
    /// Lancée si la lecture dépasse la fin des données.
    /// </exception>
    public static SoundStreamBlockTag Read(SwfReader reader, int end)
    {
        return new SoundStreamBlockTag(reader.ReadBytesTo(end));
    }
}
