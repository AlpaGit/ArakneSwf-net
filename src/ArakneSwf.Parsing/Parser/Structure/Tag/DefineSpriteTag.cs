using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineSprite (TYPE = 39).
/// </summary>
public sealed class DefineSpriteTag
{
    public const int TYPE = 39;

    public int SpriteId { get; }
    public int FrameCount { get; }
    public IReadOnlyList<object> Tags { get; }

    public DefineSpriteTag(int spriteId, int frameCount, IReadOnlyList<object> tags)
    {
        SpriteId = spriteId;
        FrameCount = frameCount;
        Tags = tags;
    }

    /// <summary>
    /// Read a DefineSprite tag.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of this tag's body.</param>
    /// <param name="swfVersion">SWF file version.</param>
    /// <param name="end">End byte offset (exclusive) of this tag in the stream.</param>
    public static DefineSpriteTag Read(SwfReader reader, int swfVersion, int end)
    {
        // If INVALID_TAG bit is NOT set, we ignore tag-parse errors (skip invalid tags).
        var ignoreTagError = (reader.Errors & Errors.InvalidTag) == 0;

        int spriteId = reader.ReadUi16();
        int frameCount = reader.ReadUi16();

        var tags = new List<object>();

        foreach (var tag in SwfTag.ReadAll(reader, end, parseId: false))
        {
            try
            {
                tags.Add(tag.Parse(reader, swfVersion));
            }
            catch (Exception ex)
            {
                if(!ignoreTagError)
                {
                    throw;
                }
                // Swallow parse errors for invalid tags if requested by error flags.
            }
        }

        return new DefineSpriteTag(spriteId, frameCount, tags);
    }
}