using System.Globalization;
using System.Text;
using System.Xml.Linq;
using ArakneSwf.Parsing.Extractor.Shapes;

namespace ArakneSwf.Parsing.Extractor.Drawer.Svg;

/// <summary>
/// Draw a path tag in an SVG element.
/// This class only fills the "d" attribute of the element.
/// </summary>
public sealed class SvgPathDrawer : IPathDrawer
{
    private readonly XElement _element;
    private readonly StringBuilder _d = new StringBuilder();

    public SvgPathDrawer(XElement element)
    {
        _element = element ?? throw new ArgumentNullException(nameof(element));
    }

    public void Move(int x, int y)
    {
        _d.Append('M')
          .Append((x / 20.0).ToString(CultureInfo.InvariantCulture))
          .Append(' ')
          .Append((y / 20.0).ToString(CultureInfo.InvariantCulture));
    }

    public void Line(int toX, int toY)
    {
        _d.Append('L')
          .Append((toX / 20.0).ToString(CultureInfo.InvariantCulture))
          .Append(' ')
          .Append((toY / 20.0).ToString(CultureInfo.InvariantCulture));
    }

    public void Curve(int controlX, int controlY, int toX, int toY)
    {
        _d.Append('Q')
          .Append((controlX / 20.0).ToString(CultureInfo.InvariantCulture))
          .Append(' ')
          .Append((controlY / 20.0).ToString(CultureInfo.InvariantCulture))
          .Append(' ')
          .Append((toX / 20.0).ToString(CultureInfo.InvariantCulture))
          .Append(' ')
          .Append((toY / 20.0).ToString(CultureInfo.InvariantCulture));
    }

    public void Draw()
    {
        _element.SetAttributeValue("d", _d.ToString());
        _d.Clear();
    }
}