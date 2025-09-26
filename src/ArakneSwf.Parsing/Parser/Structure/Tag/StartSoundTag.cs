using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// StartSound (TYPE = 15)
/// </summary>
public sealed class StartSoundTag
{
    public const int TYPE = 15;

    public int SoundId { get; }
    public SoundInfo SoundInfo { get; }

    public StartSoundTag(int soundId, SoundInfo soundInfo)
    {
        SoundId = soundId;
        SoundInfo = soundInfo;
    }

    /// <summary>
    /// Read a StartSound tag from the SWF reader.
    /// </summary>
    /// <exception cref="ParserOutOfBoundException" />
    public static StartSoundTag Read(SwfReader reader)
    {
        var soundId = reader.ReadUi16();
        var soundInfo = SoundInfo.Read(reader);
        return new StartSoundTag(soundId, soundInfo);
    }
}
