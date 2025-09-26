using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Extractor.Drawer.Svg;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Extractor.Timelines;

/// <summary>
/// Movie timeline for a sprite or a SWF file.
/// </summary>
public sealed class Timeline : IDrawable
{
    /// <summary>Display rectangle (should be identical across all frames).</summary>
    private readonly Rectangle _bounds;

    /// <summary>Frames, in order.</summary>
    private readonly Frame[] _frames;

    /// <summary>Exposes frames as read-only.</summary>
    public IReadOnlyList<Frame> Frames => Array.AsReadOnly(_frames);

    /// <summary>
    /// Create a timeline.
    /// </summary>
    /// <param name="bounds">Display rectangle (same for every frame).</param>
    /// <param name="frames">At least one frame is required.</param>
    public Timeline(Rectangle bounds, params Frame[] frames)
    {
        if (frames == null || frames.Length == 0)
            throw new ArgumentException("Timeline must contain at least one frame.", nameof(frames));

        _bounds = bounds;
        _frames = frames;
    }

    // IDrawable ---------------------------------------------------------------

    public Rectangle Bounds() => _bounds;

    public int FramesCount(bool recursive = false)
    {
        var count = _frames.Length;
        if (!recursive) return count;

        // Include nested frames; add the index so later frames advance the total.
        for (int i = 0; i < _frames.Length; i++)
        {
            var frameCount = _frames[i].FramesCount(true) + i;
            if (frameCount > count) count = frameCount;
        }

        return count;
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        var idx = Math.Min(frame, _frames.Length - 1);
        return _frames[idx].Draw(drawer, frame);
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        var transformed = new Frame[_frames.Length];
        for (int i = 0; i < _frames.Length; i++)
            transformed[i] = (Frame)_frames[i].TransformColors(colorTransform); // result is still a Frame

        return new Timeline(_bounds, transformed);
    }

    // Helpers ----------------------------------------------------------------

    /// <summary>Return a copy with updated display bounds (propagated to all frames).</summary>
    public Timeline WithBounds(Rectangle newBounds)
    {
        var updated = new Frame[_frames.Length];
        for (int i = 0; i < _frames.Length; i++)
            updated[i] = _frames[i].WithBounds(newBounds);

        return new Timeline(newBounds, updated);
    }

    /// <summary>
    /// Render one frame to SVG. If <paramref name="frame"/> exceeds available frames,
    /// the last frame is rendered.
    /// </summary>
    public string ToSvg(int frame = 0)
    {
        var idx = Math.Min(frame, _frames.Length - 1);
        var toRender = _frames[idx];

        var canvas = new SvgCanvas(toRender.Bounds());
        toRender.Draw(canvas, frame);
        return canvas.Render();
    }

    /// <summary>
    /// Render all frames to SVG, yielding (frameIndex, svg) pairs.
    /// </summary>
    public IEnumerable<(int Frame, string Svg)> ToSvgAll()
    {
        for (int f = 0; f < _frames.Length; f++)
        {
            var canvas = new SvgCanvas(_frames[f].Bounds());
            _frames[f].Draw(canvas, f);
            yield return (f, (string)canvas.Render());
        }
    }

    /// <summary>
    /// Create an empty timeline with a single empty 0×0 frame.
    /// </summary>
    public static Timeline Empty()
    {
        var emptyRect = new Rectangle(0, 0, 0, 0);
        var emptyFrame = new Frame(
            bounds: emptyRect,
            objects: new Dictionary<int, FrameObject>(),
            actions: new List<DoActionTag>(),
            label: null
        );

        return new Timeline(emptyRect, emptyFrame);
    }
}