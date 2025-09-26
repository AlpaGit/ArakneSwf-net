namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// Video frame payload for a DefineVideoStream stream.
/// </summary>
public sealed class VideoFrameTag
{
    public const int TYPE = 61;

    public int StreamId { get; }
    public int FrameNum { get; }
    public byte[] VideoData { get; }

    public VideoFrameTag(int streamId, int frameNum, byte[] videoData)
    {
        StreamId = streamId;
        FrameNum = frameNum;
        VideoData = videoData;
    }

    /// <summary>
    /// Read a VideoFrame tag from the SWF reader.
    /// </summary>
    /// <param name="reader">The SWF reader.</param>
    /// <param name="end">End byte offset of the tag payload.</param>
    /// <exception cref="ParserOutOfBoundException"></exception>
    public static VideoFrameTag Read(SwfReader reader, int end)
    {
        return new VideoFrameTag(
            streamId: reader.ReadUi16(),
            frameNum: reader.ReadUi16(),
            videoData: reader.ReadBytesTo(end)
        );
    }
}
