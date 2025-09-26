using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Shape;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

public interface IDefineShapeTag
{
    Rectangle ShapeBounds { get; }
    ShapeWithStyle Shapes { get; }
}

/// <summary>
/// DefineShape (v1=2, v2=22, v3=32).
/// </summary>
public sealed class DefineShapeTag : IDefineShapeTag
{
    public const int TYPE_V1 = 2;
    public const int TYPE_V2 = 22;
    public const int TYPE_V3 = 32;

    public int Version { get; }
    public int ShapeId { get; }
    public Rectangle ShapeBounds { get; }
    public ShapeWithStyle Shapes { get; }

    public DefineShapeTag(int version, int shapeId, Rectangle shapeBounds, ShapeWithStyle shapes)
    {
        Version = version;
        ShapeId = shapeId;
        ShapeBounds = shapeBounds;
        Shapes = shapes;
    }

    /// <summary>
    /// Read a DefineShape tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="version">Tag version: 1, 2, or 3.</param>
    public static DefineShapeTag Read(SwfReader reader, int version)
    {
        int shapeId = reader.ReadUi16();
        var shapeBounds = Rectangle.Read(reader);
        var shapes = ShapeWithStyle.Read(reader, version);

        return new DefineShapeTag(version, shapeId, shapeBounds, shapes);
    }
}