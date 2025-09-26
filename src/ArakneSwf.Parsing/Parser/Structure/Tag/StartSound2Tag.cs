using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// StartSound2 (TYPE = 89)
/// </summary>
public sealed class StartSound2Tag
{
    public const int TYPE = 89;

    public string SoundClassName { get; }
    public SoundInfo SoundInfo { get; }

    public StartSound2Tag(string soundClassName, SoundInfo soundInfo)
    {
        SoundClassName = soundClassName;
        SoundInfo = soundInfo;
    }

    /// <summary>
    /// Read a StartSound2 tag from the SWF reader.
    /// </summary>
    /// <exception cref="ParserOutOfBoundException" />
    /// <exception cref="ParserInvalidDataException" />
    public static StartSound2Tag Read(SwfReader reader)
    {
        var soundClassName = reader.ReadNullTerminatedString();
        var soundInfo = SoundInfo.Read(reader);
        return new StartSound2Tag(soundClassName, soundInfo);
    }
}
