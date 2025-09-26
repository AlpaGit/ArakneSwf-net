using System.Buffers.Binary;
using System.Text;
using ArakneSwf.Parsing.Avm;
using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Extractor;
using ArakneSwf.Parsing.Extractor.Timelines;
using ArakneSwf.Parsing.Parser;
using ArakneSwf.Parsing.Parser.Structure;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing;

/// <summary>
/// Facade for extracting information from a SWF file.
/// </summary>
public sealed class SwfFile
{
    public const int MaxFrameRate = 120;

    private Swf? _parser;

    /// <summary>
    /// Path to the SWF file on disk.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Error-reporting flags (bitfield). See <see cref="Errors"/>.
    /// </summary>
    public Errors Errors { get; }

    public SwfFile(string path, Errors errors = Errors.All)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Errors = errors;
    }

    /// <summary>
    /// Check if the file looks like a valid SWF by reading only the header.
    /// (Content may still be corrupted or incomplete.)
    /// </summary>
    /// <param name="maxLength">Maximum uncompressed length allowed, in bytes.</param>
    public bool Valid(int maxLength = 512_000_000)
    {
        // Read the first 8 bytes only (signature + version + length)
        Span<byte> head = stackalloc byte[8];
        using (var fs = File.OpenRead(Path))
        {
            var read = fs.Read(head);
            if (read < 8) return false;
        }

        var signature = Encoding.ASCII.GetString(head[..3]);
        if (signature != "CWS" && signature != "FWS")
            return false;

        var version = head[3];
        // Last known SWF version is 51; reject clearly bogus future values.
        if (version > 60)
            return false;

        var lengthLE = BinaryPrimitives.ReadUInt32LittleEndian(head[4..8]);
        if (lengthLE > maxLength)
            return false;

        return true;
    }

    /// <summary>Get the SWF file header.</summary>
    public SwfHeader Header() => Parser().Header;

    /// <summary>Get the display bounds (frame size) of this SWF.</summary>
    public Rectangle DisplayBounds() => Parser().Header.FrameSize;

    /// <summary>
    /// Get the frame rate clamped to a reasonable range.
    /// If the declared rate is &lt;= 0, returns <see cref="MaxFrameRate"/>.
    /// </summary>
    public int FrameRate()
    {
        var rate = (int)Parser().Header.FrameRate;
        if (rate <= 0) return MaxFrameRate;
        return rate > MaxFrameRate ? MaxFrameRate : rate;
    }

    /// <summary>
    /// Extract and parse tags from the file. If <paramref name="tagIds"/> is empty, returns all tags.
    /// The sequence yields tuples of (raw tag position/info, parsed tag object).
    /// </summary>
    public IEnumerable<(SwfTag Pos, object Tag)> Tags(params int[] tagIds)
    {
        var parser = Parser();
        var ignoreInvalid = (Errors & Errors.InvalidTag) == 0;

        var filter = (tagIds is { Length: > 0 })
            ? new HashSet<int>(tagIds)
            : null;

        foreach (var pos in parser.Tags) // pos is SwfTag (contains Type, Id, offsets, etc.)
        {
            if (filter is null || filter.Contains(pos.Type))
            {
                object parsed;
                try
                {
                    parsed = parser.Parse(pos);
                }
                catch (Exception e )
                {
                    if (!ignoreInvalid) throw;
                    continue;
                }

                yield return (pos, parsed);
            }
        }
    }

    /// <summary>
    /// Execute all DoAction tags and return the final VM state.
    /// NOTE: Function calls are disabled by default for safety.
    /// </summary>
    public State Execute(State? state = null, Processor? processor = null)
    {
        processor ??= new Processor(allowFunctionCall: false);
        state ??= new State();

        foreach (var (_, tag) in Tags(DoActionTag.TYPE))
        {
            if (tag is DoActionTag doAction)
            {
                state = processor.Run(doAction.Actions, state);
            }
        }

        return state;
    }

    /// <summary>
    /// Execute DoAction tags and return the global variables dictionary.
    /// </summary>
    public Dictionary<string, object?> Variables(State? state = null, Processor? processor = null)
        => Execute(state, processor).Variables;

    /// <summary>
    /// Extract an asset by its exported name.
    /// If you plan to extract several assets, prefer using <see cref="Extractor.SwfExtractor"/> directly
    /// to benefit from caching.
    /// </summary>
    public IDrawable AssetByName(string name)
    {
        var extractor = new SwfExtractor(this);
        return extractor.ByName(name);
    }

    /// <summary>
    /// Extract an asset by its character id.
    /// If not found, returns a <see cref="MissingCharacter"/>.
    /// </summary>
    public IDrawable AssetById(int id)
    {
        var extractor = new SwfExtractor(this);
        return extractor.Character(id);
    }

    /// <summary>
    /// Get all exported assets as a dictionary keyed by their exported names.
    /// </summary>
    public Dictionary<object, IDrawable> ExportedAssets()
    {
        var extractor = new SwfExtractor(this);
        var result = new Dictionary<object, IDrawable>();

        foreach (var kv in extractor.Exported())
        {
            var name = kv.Key; // may be string or number (kept as object)
            var id = kv.Value;
            result[name] = extractor.Character(id);
        }

        return result;
    }

    /// <summary>
    /// Get the root SWF timeline animation.
    /// </summary>
    /// <param name="useFileDisplayBounds">
    /// If true, the timeline is adjusted to the file display bounds; otherwise it keeps the max of all frame bounds.
    /// </param>
    public Timeline Timeline(bool useFileDisplayBounds = true)
    {
        var extractor = new SwfExtractor(this);
        return extractor.Timeline(useFileDisplayBounds);
    }

    // -- internals ------------------------------------------------------

    private Swf Parser()
    {
        if (_parser != null) return _parser;

        var bytes = File.ReadAllBytes(Path);
        // Adjust factory name if your parser uses a different constructor:
        // e.g. Swf.FromBytes(bytes, Errors) / Swf.Load(bytes, Errors) etc.
        _parser = Swf.FromBytes(bytes, Errors);
        return _parser;
    }
}
