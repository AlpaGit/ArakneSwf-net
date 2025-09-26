using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Filters;

namespace ArakneSwf.Parsing.Extractor.Timelines;

/// <summary>
/// Single object displayed in a frame.
/// </summary>
public sealed class FrameObject
{
    /// <summary>The character id of the object.</summary>
    public int CharacterId { get; }

    /// <summary>
    /// The depth of the object. Higher depth is drawn after lower depth (on top).
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// The object to draw (may differ from original if a color transform is applied).
    /// </summary>
    public IDrawable Object { get; }

    /// <summary>Bounds of the object after applying the matrix.</summary>
    public Rectangle Bounds { get; }

    /// <summary>The transformation matrix to apply to the object.</summary>
    public Matrix Matrix { get; }

    /// <summary>Color transformation to apply first (can be changed by PlaceObject move).</summary>
    public ColorTransform? ColorTransform { get; }

    /// <summary>
    /// If set, defines this object as a clipping mask up to this depth (exclusive).
    /// When set, the current object is not drawn.
    /// </summary>
    public int? ClipDepth { get; }

    /// <summary>Instance name (unique within the frame).</summary>
    public string? Name { get; }

    /// <summary>Optional filters applied to this object.</summary>
    public IReadOnlyList<Filter>? Filters { get; }

    /// <summary>Blend mode for this object.</summary>
    public BlendMode BlendMode { get; }

    // Additional color transforms applied lazily (after ColorTransform), kept internal.
    private readonly List<ColorTransform> _colorTransforms;

    public FrameObject(
        int                          characterId,
        int                          depth,
        IDrawable                    @object,
        Rectangle                    bounds,
        Matrix                       matrix,
        ColorTransform?              colorTransform  = null,
        int?                         clipDepth       = null,
        string?                      name            = null,
        IEnumerable<Filter>?         filters         = null,
        BlendMode                    blendMode       = BlendMode.Normal,
        IEnumerable<ColorTransform>? colorTransforms = null
    )
    {
        CharacterId = characterId;
        Depth = depth;
        Object = @object;
        Bounds = bounds;
        Matrix = matrix;
        ColorTransform = colorTransform;
        ClipDepth = clipDepth;
        Name = name;
        Filters = filters is null ? null : new List<Filter>(filters);
        BlendMode = blendMode;
        _colorTransforms = colorTransforms is null
            ? new List<ColorTransform>()
            : new List<ColorTransform>(colorTransforms);
    }

    /// <summary>Get the drawable with all color transforms applied (in order).</summary>
    public IDrawable TransformedObject()
    {
        var obj = Object;

        if (ColorTransform is not null)
            obj = obj.TransformColors(ColorTransform);

        // Apply lazy transforms sequentially (clamping occurs per transform).
        foreach (var t in _colorTransforms)
            obj = obj.TransformColors(t);

        return obj;
    }

    /// <summary>
    /// Return a new <see cref="FrameObject"/> with an extra color transform applied lazily.
    /// </summary>
    public FrameObject TransformColors(ColorTransform colorTransform)
        => new FrameObject(
            CharacterId,
            Depth,
            Object,
            Bounds,
            Matrix,
            ColorTransform,
            ClipDepth,
            Name,
            Filters,
            BlendMode,
            Append(_colorTransforms, colorTransform)
        );

    /// <summary>
    /// Return a copy with some properties overridden.
    /// When <paramref name="characterId"/> is provided, <paramref name="object"/> must also be provided.
    /// </summary>
    public FrameObject With(
        int?                   characterId    = null,
        IDrawable?             @object        = null,
        Rectangle?             bounds         = null,
        Matrix?                matrix         = null,
        ColorTransform?        colorTransform = null,
        IReadOnlyList<Filter>? filters        = null,
        BlendMode?             blendMode      = null,
        int?                   clipDepth      = null,
        string?                name           = null
    )
    {
        if (characterId.HasValue && @object is null)
            throw new ArgumentException("When characterId is specified, object must also be provided.",
                                        nameof(@object));

        return new FrameObject(
            characterId ?? CharacterId,
            Depth,
            @object ?? Object,
            bounds ?? Bounds,
            matrix ?? Matrix,
            colorTransform ?? ColorTransform,
            clipDepth ?? ClipDepth,
            name ?? Name,
            filters ?? Filters,
            blendMode ?? BlendMode,
            _colorTransforms // keep existing lazy transforms
        );
    }

    private static IEnumerable<ColorTransform> Append(IEnumerable<ColorTransform> source, ColorTransform extra)
    {
        foreach (var s in source) yield return s;
        yield return extra;
    }
}