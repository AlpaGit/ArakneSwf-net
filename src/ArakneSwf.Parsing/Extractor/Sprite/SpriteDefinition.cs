using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Extractor.Error;
using ArakneSwf.Parsing.Extractor.Timelines;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Extractor.Sprite;

/// <summary>
/// Store an SWF sprite character.
/// </summary>
/// <remarks>Corresponds to <c>DefineSpriteTag</c>.</remarks>
public sealed class SpriteDefinition : IDrawable
{
    private Timeline? _timeline;
    private bool _processing;
    private TimelineProcessor? _processor;

    /// <summary>The character ID of the sprite.</summary>
    public int Id { get; }

    /// <summary>The raw SWF tag.</summary>
    public DefineSpriteTag Tag { get; }

    public SpriteDefinition(TimelineProcessor processor, int id, DefineSpriteTag tag)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        Id = id;
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
    }

    // Lazy timeline creation (cached)
    public Timeline DoTimeline()
    {
        if (_timeline != null)
            return _timeline;

        if (_processing)
        {
            if (_processor != null && _processor.ErrorEnabled(Errors.CircularReference))
                throw new CircularReferenceException(
                    $"Circular reference detected while processing sprite {Id}", Id);

            return _timeline = Timeline.Empty();
        }

        _processing = true;
        try
        {
            var timeline = _processor!.Process(Tag.Tags);
            // If a timeline was already set (ignored circular ref), keep it.
            _timeline ??= timeline;
        }
        finally
        {
            _processing = false;
        }

        // Break potential cycles
        _processor = null;

        return _timeline!;
    }

    // IDrawable ----------------------------------------------------------------

    public Rectangle Bounds() => DoTimeline().Bounds();

    public int FramesCount(bool recursive = false) => DoTimeline().FramesCount(recursive);

    public IDrawer Draw(IDrawer drawer, int frame = 0) => DoTimeline().Draw(drawer, frame);

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        var transformed = (Timeline)DoTimeline().TransformColors(colorTransform);
        return new SpriteDefinition(Id, Tag, transformed);
    }

    // Helpers ------------------------------------------------------------------

    /// <summary>Render the sprite to an SVG string.</summary>
    public string ToSvg(int frame = 0) => DoTimeline().ToSvg(frame);

    // Private ctor used by TransformColors to seed a known timeline
    private SpriteDefinition(int id, DefineSpriteTag tag, Timeline timeline)
    {
        Id = id;
        Tag = tag;
        _timeline = timeline;
        _processor = null;
        _processing = false;
    }
}


