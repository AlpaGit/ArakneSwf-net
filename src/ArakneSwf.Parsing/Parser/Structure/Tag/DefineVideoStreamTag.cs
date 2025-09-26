namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineVideoStream tag (TYPE = 60).
/// </summary>
public sealed class DefineVideoStreamTag
{
    public const int TYPE = 60;

    public int CharacterId { get; }
    public int NumFrames { get; }
    public int Width { get; }
    public int Height { get; }
    public int Deblocking { get; }
    public bool Smoothing { get; }
    public int CodecId { get; }

    public DefineVideoStreamTag(
        int  characterId,
        int  numFrames,
        int  width,
        int  height,
        int  deblocking,
        bool smoothing,
        int  codecId)
    {
        CharacterId = characterId;
        NumFrames = numFrames;
        Width = width;
        Height = height;
        Deblocking = deblocking;
        Smoothing = smoothing;
        CodecId = codecId;
    }

    /// <summary>
    /// Read a DefineVideoStream tag from the SWF reader.
    /// </summary>
    public static DefineVideoStreamTag Read(SwfReader reader)
    {
        int characterId = reader.ReadUi16();
        int numFrames   = reader.ReadUi16();
        int width       = reader.ReadUi16();
        int height      = reader.ReadUi16();

        reader.SkipBits(4);                     // reserved
        var deblocking = (int)reader.ReadUb(3); // videoFlagsDeblocking
        var smoothing = reader.ReadBool();     // videoFlagsSmoothing

        int codecId = reader.ReadUi8();

        return new DefineVideoStreamTag(
            characterId: characterId,
            numFrames: numFrames,
            width: width,
            height: height,
            deblocking: deblocking,
            smoothing: smoothing,
            codecId: codecId
        );
    }
}
