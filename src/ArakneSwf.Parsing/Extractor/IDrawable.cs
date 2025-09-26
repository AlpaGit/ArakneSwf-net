using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor;

/// <summary>
/// Base type for SWF characters that can be drawn.
/// </summary>
public interface IDrawable
{
    /// <summary>
    /// Size and offset of the character.
    /// </summary>
    /// <returns>Character bounds.</returns>
    /// <exception cref="SwfException">If the bounds cannot be computed.</exception>
    Rectangle Bounds();

    /// <summary>
    /// Get the number of frames contained in the character.
    /// </summary>
    /// <param name="recursive">
    /// If true, will count the frames of all children recursively.
    /// </param>
    /// <returns>A positive integer.</returns>
    /// <exception cref="SwfException">On parsing or evaluation error.</exception>
    int FramesCount(bool recursive = false);

    /// <summary>
    /// Draw the current character to the canvas.
    /// </summary>
    /// <typeparam name="TDrawer">Drawer type.</typeparam>
    /// <param name="drawer">The drawer to use.</param>
    /// <param name="frame">
    /// The frame to draw (0-based). If greater than the number of frames, the last frame is used.
    /// Must be &gt;= 0.
    /// </param>
    /// <returns>The same <paramref name="drawer"/> instance (for fluent usage).</returns>
    /// <exception cref="SwfException">On drawing error.</exception>
    IDrawer Draw(IDrawer drawer, int frame = 0);

    /// <summary>
    /// Transform the colors of the character (non-mutating).
    /// For composite characters, the transformation should be applied recursively to all children.
    /// </summary>
    /// <param name="colorTransform">Color transform to apply.</param>
    /// <returns>A new transformed character instance.</returns>
    /// <exception cref="SwfException">On transformation error.</exception>
    IDrawable TransformColors(ColorTransform colorTransform);
}