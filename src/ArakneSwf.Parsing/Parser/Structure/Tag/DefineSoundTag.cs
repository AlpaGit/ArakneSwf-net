namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineSound tag (TYPE = 14).
/// </summary>
public sealed class DefineSoundTag
{
    public const int TYPE = 14;

    public int SoundId { get; }
    public int SoundFormat { get; }
    public int SoundRate { get; }

    /// <summary>Sound size (0 = 8 bits, 1 = 16 bits). Exposed as a boolean: true = 16 bits.</summary>
    public bool Is16Bits { get; }

    /// <summary>Sound type (0 = mono, 1 = stereo). Exposed as a boolean: true = stereo.</summary>
    public bool Stereo { get; }

    public int SoundSampleCount { get; }

    /// <summary>Raw sound data payload (bytes as-is).</summary>
    public byte[] SoundData { get; }

    public DefineSoundTag(
        int    soundId,
        int    soundFormat,
        int    soundRate,
        bool   is16Bits,
        bool   stereo,
        int    soundSampleCount,
        byte[] soundData)
    {
        SoundId = soundId;
        SoundFormat = soundFormat;
        SoundRate = soundRate;
        Is16Bits = is16Bits;
        Stereo = stereo;
        SoundSampleCount = soundSampleCount;
        SoundData = soundData;
    }

    /// <summary>
    /// Read a DefineSound tag from the given reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of this tag.</param>
    public static DefineSoundTag Read(SwfReader reader, int end)
    {
        int soundId = reader.ReadUi16();

        var flags = reader.ReadUi8();
        var format = (flags >> 4) & 0x0F;         // 4 bits
        var rate = (flags >> 2) & 0x03;           // 2 bits
        var is16 = (flags & 0b0000_0010) != 0;   // SoundSize
        var stereo = (flags & 0b0000_0001) != 0; // SoundType

        var sampleCount = reader.ReadUi32();
        var data = reader.ReadBytesTo(end);

        return new DefineSoundTag(
            soundId: soundId,
            soundFormat: format,
            soundRate: rate,
            is16Bits: is16,
            stereo: stereo,
            soundSampleCount: (int)sampleCount,
            soundData: data
        );
    }
}