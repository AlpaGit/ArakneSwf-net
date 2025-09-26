namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// FileAttributes tag (TYPE = 69).
/// </summary>
public sealed class FileAttributesTag
{
    public const int TYPE = 69;

    public bool UseDirectBlit { get; }
    public bool UseGpu { get; }
    public bool HasMetadata { get; }
    public bool ActionScript3 { get; }
    public bool UseNetwork { get; }

    public FileAttributesTag(
        bool useDirectBlit,
        bool useGpu,
        bool hasMetadata,
        bool actionScript3,
        bool useNetwork)
    {
        UseDirectBlit = useDirectBlit;
        UseGpu = useGpu;
        HasMetadata = hasMetadata;
        ActionScript3 = actionScript3;
        UseNetwork = useNetwork;
    }

    /// <summary>
    /// Read a FileAttributes tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    public static FileAttributesTag Read(SwfReader reader)
    {
        int flags = reader.ReadUi8();
        // 1 bit reserved (MSB), must be 0
        var useDirectBlit = (flags & 0b01_000000) != 0;
        var useGpu = (flags & 0b00_100000) != 0;
        var hasMetadata = (flags & 0b00_010000) != 0;
        var actionScript3 = (flags & 0b00_001000) != 0;
        // Next 2 bits reserved, must be 0
        var useNetwork = (flags & 0b00_000001) != 0;

        reader.SkipBytes(3); // Reserved, must be 0

        return new FileAttributesTag(
            useDirectBlit: useDirectBlit,
            useGpu: useGpu,
            hasMetadata: hasMetadata,
            actionScript3: actionScript3,
            useNetwork: useNetwork
        );
    }
}