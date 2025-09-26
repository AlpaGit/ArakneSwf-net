using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor;

public sealed class MissingCharacter : IDrawable
{
    /// <summary>
    /// The character ID of the requested character (SwfTag::$id).
    /// </summary>
    public int Id { get; }

    public MissingCharacter(int id)
    {
        Id = id;
    }

    public Rectangle Bounds()
    {
        return new Rectangle(0, 0, 0, 0);
    }

    public int FramesCount(bool recursive = false)
    {
        return 1;
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        return drawer;
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        return this;
    }
}


