using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DefineEditText tag (TYPE = 37).
/// </summary>
public sealed class DefineEditTextTag
{
    public const int TYPE = 37;

    public int CharacterId { get; }
    public Rectangle Bounds { get; }

    public bool WordWrap { get; }
    public bool Multiline { get; }
    public bool Password { get; }
    public bool ReadOnly { get; }
    public bool AutoSize { get; }
    public bool NoSelect { get; }
    public bool Border { get; }
    public bool WasStatic { get; }
    public bool Html { get; }
    public bool UseOutlines { get; }

    public int? FontId { get; }
    public string? FontClass { get; }
    public int? FontHeight { get; }
    public Color? TextColor { get; }
    public int? MaxLength { get; }
    public EditTextLayout? Layout { get; }

    public string VariableName { get; }
    public string? InitialText { get; }

    public DefineEditTextTag(
        int             characterId,
        Rectangle       bounds,
        bool            wordWrap,
        bool            multiline,
        bool            password,
        bool            readOnly,
        bool            autoSize,
        bool            noSelect,
        bool            border,
        bool            wasStatic,
        bool            html,
        bool            useOutlines,
        int?            fontId,
        string?         fontClass,
        int?            fontHeight,
        Color?          textColor,
        int?            maxLength,
        EditTextLayout? layout,
        string          variableName,
        string?         initialText)
    {
        CharacterId = characterId;
        Bounds = bounds;

        WordWrap = wordWrap;
        Multiline = multiline;
        Password = password;
        ReadOnly = readOnly;
        AutoSize = autoSize;
        NoSelect = noSelect;
        Border = border;
        WasStatic = wasStatic;
        Html = html;
        UseOutlines = useOutlines;

        FontId = fontId;
        FontClass = fontClass;
        FontHeight = fontHeight;
        TextColor = textColor;
        MaxLength = maxLength;
        Layout = layout;
        VariableName = variableName;
        InitialText = initialText;
    }

    /// <summary>Read a DefineEditText tag from the stream.</summary>
    public static DefineEditTextTag Read(SwfReader reader)
    {
        int characterId = reader.ReadUi16();
        var bounds = Rectangle.Read(reader);

        // First flags byte
        var flags = reader.ReadUi8();
        var hasText = (flags & 0b1000_0000) != 0;
        var wordWrap = (flags & 0b0100_0000) != 0;
        var multiline = (flags & 0b0010_0000) != 0;
        var password = (flags & 0b0001_0000) != 0;
        var readOnly = (flags & 0b0000_1000) != 0;
        var hasTextColor = (flags & 0b0000_0100) != 0;
        var hasMaxLength = (flags & 0b0000_0010) != 0;
        var hasFont = (flags & 0b0000_0001) != 0;

        // Second flags byte
        flags = reader.ReadUi8();
        var hasFontClass = (flags & 0b1000_0000) != 0;
        var autoSize = (flags & 0b0100_0000) != 0;
        var hasLayout = (flags & 0b0010_0000) != 0;
        var noSelect = (flags & 0b0001_0000) != 0;
        var border = (flags & 0b0000_1000) != 0;
        var wasStatic = (flags & 0b0000_0100) != 0;
        var html = (flags & 0b0000_0010) != 0;
        var useOutlines = (flags & 0b0000_0001) != 0;

        var fontId = hasFont ? reader.ReadUi16() : (int?)null;
        var fontClass = hasFontClass ? reader.ReadNullTerminatedString() : null;
        var fontHeight = hasFont ? reader.ReadUi16() : (int?)null;
        var textColor = hasTextColor ? Color.ReadRgba(reader) : null;
        var maxLength = hasMaxLength ? reader.ReadUi16() : (int?)null;
        var layout = hasLayout ? EditTextLayout.Read(reader) : null;

        var variableName = reader.ReadNullTerminatedString();
        var initialText = hasText ? reader.ReadNullTerminatedString() : null;

        return new DefineEditTextTag(
            characterId,
            bounds,
            wordWrap,
            multiline,
            password,
            readOnly,
            autoSize,
            noSelect,
            border,
            wasStatic,
            html,
            useOutlines,
            fontId,
            fontClass,
            fontHeight,
            textColor,
            maxLength,
            layout,
            variableName,
            initialText
        );
    }
}