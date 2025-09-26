using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Extractor.Error;
using ArakneSwf.Parsing.Extractor.Images;
using ArakneSwf.Parsing.Extractor.Shapes.FillTypes;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Shape;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Extractor.Shapes;

/// <summary>
/// Process define shape action tags to create shape objects.
/// </summary>
public sealed class ShapeProcessor
{
    private readonly SwfExtractor _extractor;

    public ShapeProcessor(SwfExtractor extractor)
    {
        _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
    }

    /// <summary>
    /// Transform a DefineShapeTag or DefineShape4Tag into a Shape object.
    /// </summary>
    public Shape Process(IDefineShapeTag tag)
    {
        if (tag is null) throw new ArgumentNullException(nameof(tag));

        var b = tag.ShapeBounds;

        return new Shape(
            width: b.Width(),
            height: b.Height(),
            xOffset: -b.XMin,
            yOffset: -b.YMin,
            paths: ProcessPaths(tag)
        );
    }

    /// <summary>
    /// Build all paths for the supplied tag.
    /// </summary>
    private List<Path> ProcessPaths(IDefineShapeTag tag)
    {
        var fillStyles = tag.Shapes.FillStyles;
        var lineStyles = tag.Shapes.LineStyles;

        int x = 0, y = 0;

        PathStyle? fillStyle0 = null;
        PathStyle? fillStyle1 = null;
        PathStyle? lineStyle = null;

        var builder = new PathsBuilder();
        var edges = new List<IEdge>();

        foreach (var record in tag.Shapes.ShapeRecords)
        {
            switch (record)
            {
                case StyleChangeRecord sc:
                    builder.Merge(edges);
                    edges.Clear();

                    if (sc.Reset())
                    {
                        // Start a new drawing context
                        builder.FinalizePaths();
                    }

                    if (sc.StateNewStyles)
                    {
                        // Reset styles to ensure that we don't use old styles
                        builder.Close();

                        fillStyles = sc.FillStyles;
                        lineStyles = sc.LineStyles;
                    }

                    if (sc.StateLineStyle)
                    {
                        var style = (sc.LineStyle > 0 && sc.LineStyle <= lineStyles.Count)
                            ? lineStyles[sc.LineStyle - 1]
                            : null;

                        if (style is not null)
                        {
                            lineStyle = new PathStyle(
                                lineColor: style.Color,
                                lineFill: style.FillType is null ? null : CreateFillType(style.FillType),
                                lineWidth: style.Width
                            );
                        }
                        else
                        {
                            lineStyle = null;
                        }
                    }

                    if (sc.StateFillStyle0)
                    {
                        var style = (sc.FillStyle0 > 0 && sc.FillStyle0 <= fillStyles.Count)
                            ? fillStyles[sc.FillStyle0 - 1]
                            : null;

                        fillStyle0 = style is null
                            ? null
                            : new PathStyle(fill: CreateFillType(style), reverse: true);
                    }

                    if (sc.StateFillStyle1)
                    {
                        var style = (sc.FillStyle1 > 0 && sc.FillStyle1 <= fillStyles.Count)
                            ? fillStyles[sc.FillStyle1 - 1]
                            : null;

                        fillStyle1 = style is null
                            ? null
                            : new PathStyle(fill: CreateFillType(style));
                    }

                    builder.SetActiveStyles(fillStyle0, fillStyle1, lineStyle);

                    if (sc.StateMoveTo)
                    {
                        x = sc.MoveDeltaX;
                        y = sc.MoveDeltaY;
                    }

                    break;

                case StraightEdgeRecord se:
                {
                    var toX = x + se.DeltaX;
                    var toY = y + se.DeltaY;

                    edges.Add(new StraightEdge(x, y, toX, toY));

                    x = toX;
                    y = toY;
                    break;
                }

                case CurvedEdgeRecord ce:
                {
                    var fromX = x;
                    var fromY = y;
                    var controlX = x + ce.ControlDeltaX;
                    var controlY = y + ce.ControlDeltaY;
                    var toX = x + ce.ControlDeltaX + ce.AnchorDeltaX;
                    var toY = y + ce.ControlDeltaY + ce.AnchorDeltaY;

                    edges.Add(new CurvedEdge(fromX, fromY, controlX, controlY, toX, toY));

                    x = toX;
                    y = toY;
                    break;
                }

                case EndShapeRecord:
                    builder.Merge(edges);
                    return builder.Export();

                default:
                    // Ignore unknown record types silently (matches PHP behavior).
                    break;
            }
        }

        return builder.Export();
    }

    private IFillType CreateFillType(FillStyle style) =>
        style.Type switch
        {
            FillStyle.SOLID                         => CreateSolidFill(style),
            FillStyle.LINEAR_GRADIENT               => CreateLinearGradientFill(style),
            FillStyle.RADIAL_GRADIENT               => CreateRadialGradientFill(style, style.Gradient),
            FillStyle.FOCAL_GRADIENT                => CreateRadialGradientFill(style, style.FocalGradient),
            FillStyle.REPEATING_BITMAP              => CreateBitmapFill(style, smoothed: true, repeat: true),
            FillStyle.CLIPPED_BITMAP                => CreateBitmapFill(style, smoothed: true, repeat: false),
            FillStyle.NON_SMOOTHED_REPEATING_BITMAP => CreateBitmapFill(style, smoothed: false, repeat: true),
            FillStyle.NON_SMOOTHED_CLIPPED_BITMAP   => CreateBitmapFill(style, smoothed: false, repeat: false),
            _ => _extractor.ErrorEnabled(Errors.UnprocessableData)
                ? throw new ProcessingInvalidDataException($"Unknown fill style: {(int)style.Type}")
                : new Solid(new Color(0, 0, 0, 0)),
        };

    private Solid CreateSolidFill(FillStyle style)
    {
        var color = style.Color ?? throw new InvalidOperationException("Solid fill requires a color.");
        return new Solid(color);
    }

    private LinearGradient CreateLinearGradientFill(FillStyle style)
    {
        var matrix = style.Matrix ?? throw new InvalidOperationException("LinearGradient requires a matrix.");
        var gradient = style.Gradient ?? throw new InvalidOperationException("LinearGradient requires a gradient.");
        return new LinearGradient(matrix, gradient);
    }

    private RadialGradient CreateRadialGradientFill(FillStyle style, Gradient? gradient)
    {
        var matrix = style.Matrix ?? throw new InvalidOperationException("RadialGradient requires a matrix.");
        if (gradient is null) throw new InvalidOperationException("RadialGradient requires a gradient.");
        return new RadialGradient(matrix, gradient);
    }

    private Bitmap CreateBitmapFill(FillStyle style, bool smoothed, bool repeat)
    {
        var bitmapId = style.BitmapId ?? throw new InvalidOperationException("Bitmap fill requires a bitmap id.");
        var matrix = style.BitmapMatrix ?? throw new InvalidOperationException("Bitmap fill requires a bitmap matrix.");

        var character = _extractor.Character(bitmapId);

        if (character is not IImageCharacter image)
        {
            if (_extractor.ErrorEnabled(Errors.UnprocessableData))
            {
                throw new ProcessingInvalidDataException($"The character {bitmapId} is not a valid image character");
            }

            image = new EmptyImage(bitmapId);
        }

        return new Bitmap(image, matrix, smoothed: smoothed, repeat: repeat);
    }
}