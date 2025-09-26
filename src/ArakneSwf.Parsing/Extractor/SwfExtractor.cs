using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Extractor.Images;
using ArakneSwf.Parsing.Extractor.Shapes;
using ArakneSwf.Parsing.Extractor.Sprite;
using ArakneSwf.Parsing.Extractor.Timelines;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Extractor;

/// <summary>
/// Extract resources from a SWF file.
/// </summary>
public sealed class SwfExtractor
{
    private readonly SwfFile _file;

    // Cache dictionaries (lazy-filled)
    private Dictionary<int, object /* ShapeDefinition|SpriteDefinition|IImageCharacter */>? _characters;
    private Dictionary<int, ShapeDefinition>? _shapes;
    private Dictionary<int, SpriteDefinition>? _sprites;
    private Dictionary<int, IImageCharacter>? _images;
    private Dictionary<string, int>? _exported;

    private Timeline? _timeline;

    public SwfExtractor(SwfFile file)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }

    /// <summary>
    /// Check if the given error is enabled.
    /// </summary>
    /// <param name="error">One of SwfFile.ERROR_* constants.</param>
    public bool ErrorEnabled(Errors error) => (_file.Errors & error) != 0;

    /// <summary>
    /// Extract all shapes (indexed by characterId).
    /// Shapes are processed lazily when needed.
    /// </summary>
    /// <exception cref="ParserException">Thrown if the parser fails.</exception>
    public IReadOnlyDictionary<int, ShapeDefinition> Shapes()
    {
        if (_shapes is not null) return _shapes;

        var map = new Dictionary<int, ShapeDefinition>();
        var processor = new ShapeProcessor(this);

        foreach (var entry in _file.Tags(DefineShapeTag.TYPE_V1,
                                         DefineShapeTag.TYPE_V2,
                                         DefineShapeTag.TYPE_V3,
                                         DefineShape4Tag.TYPE_V4))
        {
            var id = entry.Pos.Id;
            if (id is null) continue;

            switch (entry.Tag)
            {
                case DefineShapeTag s:
                    map[id.Value] = new ShapeDefinition(processor, id.Value, s);
                    break;
                case DefineShape4Tag s4:
                    map[id.Value] = new ShapeDefinition(processor, id.Value, s4);
                    break;
            }
        }

        _shapes = map;
        return _shapes;
    }

    /// <summary>
    /// Extract all raster images (indexed by characterId).
    /// </summary>
    /// <exception cref="ParserException">Thrown if the parser fails.</exception>
    public IReadOnlyDictionary<int, IImageCharacter> Images()
    {
        if (_images is not null) return _images;

        var result = new Dictionary<int, IImageCharacter>();

        foreach (var kv in ExtractLosslessImages())
            result[kv.Key] = kv.Value;

        foreach (var kv in ExtractJpeg())
            result[kv.Key] = kv.Value;

        foreach (var kv in ExtractDefineBits())
            result[kv.Key] = kv.Value;

        _images = result;
        return _images;
    }

    /// <summary>
    /// Extract all sprites (indexed by characterId).
    /// </summary>
    /// <exception cref="ParserException">Thrown if the parser fails.</exception>
    public IReadOnlyDictionary<int, SpriteDefinition> Sprites()
    {
        if (_sprites is not null) return _sprites;

        var map = new Dictionary<int, SpriteDefinition>();
        var processor = new TimelineProcessor(this);

        foreach (var entry in _file.Tags(DefineSpriteTag.TYPE))
        {
            var id = entry.Pos.Id;
            if (id is null) continue;

            if (entry.Tag is DefineSpriteTag spriteTag)
            {
                map[id.Value] = new SpriteDefinition(processor, id.Value, spriteTag);
            }
        }

        _sprites = map;
        return _sprites;
    }

    /// <summary>
    /// Get the root SWF timeline animation.
    /// </summary>
    /// <param name="useFileDisplayBounds">
    /// If true, adjust the timeline to the file display bounds; otherwise keep the max frame bounds.
    /// </param>
    /// <exception cref="ParserException">Thrown if the parser fails.</exception>
    public Timeline Timeline(bool useFileDisplayBounds = true)
    {
        if (_timeline is null)
        {
            var processor = new TimelineProcessor(this);
            _timeline = processor.Process(_file.Tags(TimelineProcessor.TAG_TYPES).Select(x => x.Tag));
        }

        if (!useFileDisplayBounds) return _timeline;
        return _timeline.WithBounds(_file.DisplayBounds());
    }

    /// <summary>
    /// Get a SWF character by its ID. Returns a MissingCharacter if not found.
    /// </summary>
    /// <exception cref="ParserException">Thrown if the parser fails.</exception>
    public IDrawable Character(int characterId)
    {
        _characters ??= new Dictionary<int, object>();

        // First fill caches if empty (combine shapes + sprites + images)
        if (_characters.Count == 0)
        {
            foreach (var kv in Shapes())
                _characters[kv.Key] = kv.Value;

            foreach (var kv in Sprites())
                _characters[kv.Key] = kv.Value;

            foreach (var kv in Images())
                _characters[kv.Key] = kv.Value;
        }

        return (IDrawable)(_characters.TryGetValue(characterId, out var value)
            ? value
            : new MissingCharacter(characterId));
    }

    /// <summary>
    /// Get a character by its exported name.
    /// </summary>
    /// <exception cref="ArgumentException">If the name has not been exported.</exception>
    /// <exception cref="ParserException">Thrown if the parser fails.</exception>
    public IDrawable ByName(string name)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));

        var map = Exported();
        if (!map.TryGetValue(name, out var id))
            throw new ArgumentException($"The name \"{name}\" has not been exported.", nameof(name));

        return Character(id);
    }

    /// <summary>
    /// Get all exported names mapped to character IDs.
    /// </summary>
    /// <exception cref="ParserException">Thrown if the parser fails.</exception>
    public IReadOnlyDictionary<string, int> Exported()
    {
        if (_exported is not null) return _exported;

        var exported = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var entry in _file.Tags(SymbolClassTag.TYPE))
        {
            if (entry.Tag is not SymbolClassTag tag) continue;

            foreach (var kv in tag.Symbols)
            {
                var name = kv.Value;
                exported[name] = kv.Key;
            }
        }

        foreach (var entry in _file.Tags(ExportAssetsTag.ID))
        {
            if (entry.Tag is not ExportAssetsTag tag) continue;

            foreach (var kv in tag.Characters)
            {
                var name = kv.Value ?? string.Empty;
                exported[name] = kv.Key;
            }
        }

        _exported = exported;
        return _exported;
    }

    /// <summary>
    /// Release all loaded resources to help GC (can still be reused later).
    /// </summary>
    public void Release()
    {
        _characters = null;
        _sprites = null;
        _images = null;
        _shapes = null;
        _exported = null;
        _timeline = null;
    }

    /// <exception cref="ParserException">Thrown if the parser fails.</exception>
    private Dictionary<int, LosslessImageDefinition> ExtractLosslessImages()
    {
        var images = new Dictionary<int, LosslessImageDefinition>();

        foreach (var entry in _file.Tags(DefineBitsLosslessTag.TYPE_V1, DefineBitsLosslessTag.TYPE_V2))
        {
            var id = entry.Pos.Id;
            if (id is null) continue;

            if (entry.Tag is DefineBitsLosslessTag tag)
            {
                images[id.Value] = new LosslessImageDefinition(tag);
            }
        }

        return images;
    }

    /// <exception cref="ParserException">Thrown if the parser fails.</exception>
    private Dictionary<int, ImageBitsDefinition> ExtractDefineBits()
    {
        var images = new Dictionary<int, ImageBitsDefinition>();
        JpegTablesTag? jpegTables = null;

        foreach (var entry in _file.Tags(JpegTablesTag.TYPE, DefineBitsTag.TYPE))
        {
            switch (entry.Tag)
            {
                case JpegTablesTag jt:
                    jpegTables = jt;
                    break;

                case DefineBitsTag bits when jpegTables is not null:
                    images[bits.CharacterId] = new ImageBitsDefinition(bits, jpegTables);
                    break;
            }
        }

        return images;
    }

    /// <exception cref="ParserException">Thrown if the parser fails.</exception>
    private Dictionary<int, JpegImageDefinition> ExtractJpeg()
    {
        var images = new Dictionary<int, JpegImageDefinition>();

        foreach (var entry in _file.Tags(DefineBitsJpeg2Tag.TYPE, DefineBitsJpeg3Tag.TYPE, DefineBitsJpeg4Tag.TYPE))
        {
            var id = entry.Pos.Id;
            if (id is null) continue;

            switch (entry.Tag)
            {
                case DefineBitsJpeg2Tag t2:
                    images[id.Value] = new JpegImageDefinition(t2);
                    break;
                case DefineBitsJpeg3Tag t3:
                    images[id.Value] = new JpegImageDefinition(t3);
                    break;
                case DefineBitsJpeg4Tag t4:
                    images[id.Value] = new JpegImageDefinition(t4);
                    break;
            }
        }

        return images;
    }
}