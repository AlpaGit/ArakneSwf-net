namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// CSMTextSettings tag (ID = 74).
/// </summary>
public sealed class CsmTextSettingsTag
{
    public const int ID = 74;

    public int TextId { get; }
    public int UseFlashType { get; } // 2 bits
    public int GridFit { get; }      // 3 bits
    public float Thickness { get; }
    public float Sharpness { get; }

    public CsmTextSettingsTag(int textId, int useFlashType, int gridFit, float thickness, float sharpness)
    {
        TextId = textId;
        UseFlashType = useFlashType;
        GridFit = gridFit;
        Thickness = thickness;
        Sharpness = sharpness;
    }

    /// <summary>
    /// Read a CSMTextSettings tag from the SWF reader.
    /// </summary>
    public static CsmTextSettingsTag Read(SwfReader reader)
    {
        int textId = reader.ReadUi16();
        var useFlashType = (int)reader.ReadUb(2);
        var gridFit = (int)reader.ReadUb(3);
        reader.SkipBits(3); // reserved
        var thickness = reader.ReadFloat();
        var sharpness = reader.ReadFloat();
        reader.SkipBytes(1); // reserved

        return new CsmTextSettingsTag(textId, useFlashType, gridFit, thickness, sharpness);
    }
}