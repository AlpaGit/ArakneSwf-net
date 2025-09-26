namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineSceneAndFrameLabelData tag (TYPE = 86).
/// </summary>
public sealed class DefineSceneAndFrameLabelDataTag
{
    public const int TYPE = 86;

    public IReadOnlyList<int> SceneOffsets { get; }
    public IReadOnlyList<string> SceneNames { get; }
    public IReadOnlyList<int> FrameNumbers { get; }
    public IReadOnlyList<string> FrameLabels { get; }

    public DefineSceneAndFrameLabelDataTag(
        IReadOnlyList<int>    sceneOffsets,
        IReadOnlyList<string> sceneNames,
        IReadOnlyList<int>    frameNumbers,
        IReadOnlyList<string> frameLabels)
    {
        SceneOffsets = sceneOffsets;
        SceneNames = sceneNames;
        FrameNumbers = frameNumbers;
        FrameLabels = frameLabels;
    }

    /// <summary>
    /// Read a DefineSceneAndFrameLabelData tag from the reader.
    /// </summary>
    public static DefineSceneAndFrameLabelDataTag Read(SwfReader reader)
    {
        var sceneOffsets = new List<int>();
        var sceneNames = new List<string>();

        var sceneCount = (int)reader.ReadEncodedU32();
        for (var i = 0; i < sceneCount && reader.Offset < reader.End; i++)
        {
            sceneOffsets.Add((int)reader.ReadEncodedU32());
            sceneNames.Add(reader.ReadNullTerminatedString());
        }

        var frameNumbers = new List<int>();
        var frameLabels = new List<string>();

        var frameLabelCount = (int)reader.ReadEncodedU32();
        for (var i = 0; i < frameLabelCount && reader.Offset < reader.End; i++)
        {
            frameNumbers.Add((int)reader.ReadEncodedU32());
            frameLabels.Add(reader.ReadNullTerminatedString());
        }

        return new DefineSceneAndFrameLabelDataTag(
            sceneOffsets,
            sceneNames,
            frameNumbers,
            frameLabels
        );
    }
}