namespace ArakneSwf.Parsing.Parser.Structure.Record;

/// <summary>
/// EditText layout information (alignment, margins, indent, leading).
/// </summary>
public sealed class EditTextLayout
{
    public int Align { get; }
    public int LeftMargin { get; }
    public int RightMargin { get; }
    public int Indent { get; }
    public int Leading { get; }

    public EditTextLayout(int align, int leftMargin, int rightMargin, int indent, int leading)
    {
        Align = align;
        LeftMargin = leftMargin;
        RightMargin = rightMargin;
        Indent = indent;
        Leading = leading;
    }

    /// <summary>Reads an <see cref="EditTextLayout"/> from the SWF stream.</summary>
    public static EditTextLayout Read(SwfReader reader)
    {
        int align = reader.ReadUi8();
        int leftMargin = reader.ReadUi16();
        int rightMargin = reader.ReadUi16();
        int indent = reader.ReadUi16();
        int leading = reader.ReadSi16();

        return new EditTextLayout(align, leftMargin, rightMargin, indent, leading);
    }
}