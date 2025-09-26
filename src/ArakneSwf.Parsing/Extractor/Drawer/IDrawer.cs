using ArakneSwf.Parsing.Extractor.Images;
using ArakneSwf.Parsing.Extractor.Shapes;
using ArakneSwf.Parsing.Extractor.Timelines;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Filters;
using Path = ArakneSwf.Parsing.Extractor.Shapes.Path;

namespace ArakneSwf.Parsing.Extractor.Drawer;

/// <summary>
/// Rendering / drawing interface.
/// </summary>
public interface IDrawer
{
    /// <summary>
    /// Start a new drawing area.
    /// </summary>
    /// <param name="bounds">Drawing bounds (in twips).</param>
    void Area(Rectangle bounds);

    /// <summary>
    /// Draw a new shape.
    /// </summary>
    void Shape(Shape shape);

    /// <summary>
    /// Draw a raster image.
    /// </summary>
    void Image(IImageCharacter image);

    /// <summary>
    /// Include a sprite or shape in the current drawing.
    /// </summary>
    /// <param name="object">Drawable object to include.</param>
    /// <param name="matrix">Transform to apply.</param>
    /// <param name="frame">Frame to draw (0-based, must be &gt;= 0).</param>
    /// <param name="filters">Optional filter list applied to the object.</param>
    /// <param name="blendMode">Blend mode to use.</param>
    /// <param name="name">Optional instance name.</param>
    void Include(
        IDrawable              @object,
        Matrix                 matrix,
        int                    frame     = 0,
        IReadOnlyList<Filter>? filters   = null,
        BlendMode              blendMode = BlendMode.Normal,
        string?                name      = null
    );

    /// <summary>
    /// Use the given object as a clipping mask for subsequent drawing operations.
    /// Returns an id that must be passed to <see cref="EndClip"/>.
    /// </summary>
    /// <param name="object">Mask drawable.</param>
    /// <param name="matrix">Transform to apply to the mask.</param>
    /// <param name="frame">Frame to draw (0-based).</param>
    /// <returns>Clip id.</returns>
    string StartClip(IDrawable @object, Matrix matrix, int frame);

    /// <summary>
    /// Stop (remove) the given clipping mask.
    /// </summary>
    /// <param name="clipId">Clip id returned by <see cref="StartClip"/>.</param>
    void EndClip(string clipId);

    /// <summary>
    /// Draw a path.
    /// </summary>
    void Path(Path path);

    /// <summary>
    /// Render the drawing. The concrete return type depends on the implementation.
    /// </summary>
    object? Render();
}