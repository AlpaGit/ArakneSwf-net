using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineButtonSound tag (TYPE = 17).
/// Associates up to four sounds to button state transitions.
/// </summary>
public sealed class DefineButtonSoundTag
{
    public const int TYPE = 17;

    public int ButtonId { get; }
    public int ButtonSoundChar0 { get; }
    public SoundInfo? ButtonSoundInfo0 { get; }
    public int ButtonSoundChar1 { get; }
    public SoundInfo? ButtonSoundInfo1 { get; }
    public int ButtonSoundChar2 { get; }
    public SoundInfo? ButtonSoundInfo2 { get; }
    public int ButtonSoundChar3 { get; }
    public SoundInfo? ButtonSoundInfo3 { get; }

    public DefineButtonSoundTag(
        int buttonId,
        int buttonSoundChar0, SoundInfo? buttonSoundInfo0,
        int buttonSoundChar1, SoundInfo? buttonSoundInfo1,
        int buttonSoundChar2, SoundInfo? buttonSoundInfo2,
        int buttonSoundChar3, SoundInfo? buttonSoundInfo3)
    {
        ButtonId = buttonId;

        ButtonSoundChar0 = buttonSoundChar0;
        ButtonSoundInfo0 = buttonSoundInfo0;

        ButtonSoundChar1 = buttonSoundChar1;
        ButtonSoundInfo1 = buttonSoundInfo1;

        ButtonSoundChar2 = buttonSoundChar2;
        ButtonSoundInfo2 = buttonSoundInfo2;

        ButtonSoundChar3 = buttonSoundChar3;
        ButtonSoundInfo3 = buttonSoundInfo3;
    }

    /// <summary>
    /// Read a DefineButtonSound tag from the stream.
    /// </summary>
    public static DefineButtonSoundTag Read(SwfReader reader)
    {
        int buttonId = reader.ReadUi16();

        int char0 = reader.ReadUi16();
        var info0 = (char0 != 0) ? SoundInfo.Read(reader) : null;

        int char1 = reader.ReadUi16();
        var info1 = (char1 != 0) ? SoundInfo.Read(reader) : null;

        int char2 = reader.ReadUi16();
        var info2 = (char2 != 0) ? SoundInfo.Read(reader) : null;

        int char3 = reader.ReadUi16();
        var info3 = (char3 != 0) ? SoundInfo.Read(reader) : null;

        return new DefineButtonSoundTag(
            buttonId,
            char0,
            info0,
            char1,
            info1,
            char2,
            info2,
            char3,
            info3
        );
    }
}