namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// SoundStreamHead / SoundStreamHead2 tag.
/// </summary>
public sealed class SoundStreamHeadTag
{
    public const int TYPE_V1 = 18;
    public const int TYPE_V2 = 45;

    public int Version { get; }
    public int PlaybackSoundRate { get; }

    /// <summary>0 = 8 bits, 1 = 16 bits</summary>
    public int PlaybackSoundSize { get; }

    /// <summary>0 = mono, 1 = stéréo</summary>
    public int PlaybackSoundType { get; }

    public int StreamSoundCompression { get; }
    public int StreamSoundRate { get; }

    /// <summary>0 = 8 bits, 1 = 16 bits</summary>
    public int StreamSoundSize { get; }

    /// <summary>0 = mono, 1 = stéréo</summary>
    public int StreamSoundType { get; }

    public int StreamSoundSampleCount { get; }

    /// <summary>Présent uniquement si <see cref="StreamSoundCompression"/> == 2 (MP3).</summary>
    public int? LatencySeek { get; }

    public SoundStreamHeadTag(
        int  version,
        int  playbackSoundRate,
        int  playbackSoundSize,
        int  playbackSoundType,
        int  streamSoundCompression,
        int  streamSoundRate,
        int  streamSoundSize,
        int  streamSoundType,
        int  streamSoundSampleCount,
        int? latencySeek
    )
    {
        Version = version;
        PlaybackSoundRate = playbackSoundRate;
        PlaybackSoundSize = playbackSoundSize;
        PlaybackSoundType = playbackSoundType;
        StreamSoundCompression = streamSoundCompression;
        StreamSoundRate = streamSoundRate;
        StreamSoundSize = streamSoundSize;
        StreamSoundType = streamSoundType;
        StreamSoundSampleCount = streamSoundSampleCount;
        LatencySeek = latencySeek;
    }

    /// <summary>
    /// Lit un tag SoundStreamHead (v1) ou SoundStreamHead2 (v2) depuis le lecteur SWF.
    /// </summary>
    /// <param name="reader">Lecteur SWF.</param>
    /// <param name="version">Version du tag (1 ou 2).</param>
    /// <returns>Instance de <see cref="SoundStreamHeadTag"/>.</returns>
    /// <exception cref="ParserOutOfBoundException">Si la lecture dépasse la fin des données.</exception>
    public static SoundStreamHeadTag Read(SwfReader reader, int version)
    {
        // Octet 1: playback settings (4 bits réservés, puis 2 bits rate, 1 bit size, 1 bit type)
        int flags = reader.ReadUi8();
        var playbackSoundRate = (flags >> 2) & 0x03;
        var playback16Bits = (flags & 0b0000_0010) != 0;
        var playbackStereo = (flags & 0b0000_0001) != 0;

        // Octet 2: stream settings (4 bits compression, 2 bits rate, 1 bit size, 1 bit type)
        flags = reader.ReadUi8();
        var compression = (flags >> 4) & 0x0F;
        var streamSoundRate = (flags >> 2) & 0x03;
        var stream16Bits = (flags & 0b0000_0010) != 0;
        var streamStereo = (flags & 0b0000_0001) != 0;

        int streamSoundSampleCount = reader.ReadUi16();
        int? latencySeek = (compression == 2) ? reader.ReadSi16() : null;

        return new SoundStreamHeadTag(
            version: version,
            playbackSoundRate: playbackSoundRate,
            playbackSoundSize: playback16Bits ? 1 : 0,
            playbackSoundType: playbackStereo ? 1 : 0,
            streamSoundCompression: compression,
            streamSoundRate: streamSoundRate,
            streamSoundSize: stream16Bits ? 1 : 0,
            streamSoundType: streamStereo ? 1 : 0,
            streamSoundSampleCount: streamSoundSampleCount,
            latencySeek: latencySeek
        );
    }
}