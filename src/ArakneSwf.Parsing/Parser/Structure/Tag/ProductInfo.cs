namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// Attach information about the product that created the SWF file.
/// Note: this tag is not documented in the official SWF documentation.
/// See: https://www.m2osw.com/swf_tag_productinfo
/// </summary>
public sealed class ProductInfo
{
    public const int TYPE = 41;

    public int ProductId { get; }
    public int Edition { get; }
    public int MajorVersion { get; }
    public int MinorVersion { get; }
    public long BuildNumber { get; }
    public long CompilationDate { get; }

    public ProductInfo(
        int  productId,
        int  edition,
        int  majorVersion,
        int  minorVersion,
        long buildNumber,
        long compilationDate)
    {
        ProductId = productId;
        Edition = edition;
        MajorVersion = majorVersion;
        MinorVersion = minorVersion;
        BuildNumber = buildNumber;
        CompilationDate = compilationDate;
    }

    /// <summary>
    /// Read a ProductInfo tag from the given reader.
    /// </summary>
    public static ProductInfo Read(SwfReader reader)
    {
        return new ProductInfo(
            productId: (int)reader.ReadUi32(),
            edition: (int)reader.ReadUi32(),
            majorVersion: reader.ReadUi8(),
            minorVersion: reader.ReadUi8(),
            buildNumber: reader.ReadSi64(),
            compilationDate: reader.ReadSi64()
        );
    }
}
