using System.Globalization;
using ArakneSwf.Parsing.Extractor.Drawer.Converters;

namespace SwfRender.Console;

/// <summary>
/// CLI options for the swf-extract command.
/// </summary>
public sealed class ExtractOptions
{
    public const string DefaultOutputFilename = "{basename}/{name}{_frame}.{ext}";

    // ---- Constructor & properties ----

    public string Command { get; }
    public string? Error { get; }
    public bool Help { get; }

    public IReadOnlyList<string> Files { get; }
    public string Output { get; }
    public string OutputFilename { get; }

    public IReadOnlyList<int> Characters { get; }
    public IReadOnlyList<string> Exported { get; }
    public IReadOnlyList<int>? Frames { get; }

    public bool FullAnimation { get; }
    public bool AllSprites { get; }
    public bool AllExported { get; }
    public bool Timeline { get; }
    public bool Variables { get; }

    public IReadOnlyList<DrawableFormater> FrameFormat { get; }
    public IReadOnlyList<AnimationFormater> AnimationFormat { get; }

    public ExtractOptions(
        string                            command,
        string?                           error           = null,
        bool                              help            = false,
        IReadOnlyList<string>?            files           = null,
        string?                           output          = null,
        string?                           outputFilename  = null,
        IReadOnlyList<int>?               characters      = null,
        IReadOnlyList<string>?            exported        = null,
        IReadOnlyList<int>?               frames          = null,
        bool                              fullAnimation   = false,
        bool                              allSprites      = false,
        bool                              allExported     = false,
        bool                              timeline        = false,
        bool                              variables       = false,
        IReadOnlyList<DrawableFormater>?  frameFormat     = null,
        IReadOnlyList<AnimationFormater>? animationFormat = null)
    {
        Command = command;
        Error = error;
        Help = help;

        Files = files ?? [];
        Output = output ?? string.Empty;
        OutputFilename = outputFilename ?? DefaultOutputFilename;

        Characters = characters ?? [];
        Exported = exported ?? [];
        Frames = frames;

        FullAnimation = fullAnimation;
        AllSprites = allSprites;
        AllExported = allExported;
        Timeline = timeline;
        Variables = variables;

        FrameFormat = frameFormat ?? [new DrawableFormater(ImageFormat.Svg)];
        AnimationFormat = animationFormat ?? [];
    }

    // ---- Factory: mimic PHP createFromCli() ----

    /// <summary>
    /// Create options from command line args.
    /// Exits early with Help=true if no args or --help/-h provided.
    /// Returns Error message in <see cref="Error"/> when validation fails.
    /// </summary>
    public static ExtractOptions CreateFromCli(string[]? args = null, string? command = null)
    {
        var argv = args ?? Environment.GetCommandLineArgs().Skip(1).ToArray();
        var cmd = command ?? (Environment.GetCommandLineArgs().FirstOrDefault() ?? "swf-extract");

        // If no args, show help by default
        if (argv.Length == 0)
            return new ExtractOptions(cmd, help: true);

        // Parse options
        var opt = new ParsedOptions();
        var positionals = new List<string>();
        ParseArgs(argv, opt, positionals);

        // Help requested
        if (opt.Help)
            return new ExtractOptions(cmd, help: true);

        // Must have at least <file> and <output>
        if (positionals.Count < 2)
            return new ExtractOptions(cmd, error: "Not enough arguments: <file> and <output> are required");

        // Output directory is the last positional arg; the rest are input files
        var output = positionals[^1];
        var files = positionals.Take(positionals.Count - 1).ToList();

        // Create/validate output directory
        try
        {
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output); // mode 0775-like is OS-specific; rely on defaults
            }
        }
        catch
        {
            return new ExtractOptions(cmd, error: $"Cannot create output directory: {output}");
        }

        string fullOutput;
        try
        {
            fullOutput = Path.GetFullPath(output);
        }
        catch
        {
            return new ExtractOptions(cmd, error: $"Cannot resolve output directory: {output}");
        }

        // Output filename pattern
        var outputFilename = DefaultOutputFilename;
        if (opt.OutputFilename.Count > 0)
        {
            if (opt.OutputFilename.Count > 1)
                return new ExtractOptions(cmd, error: "The --output-filename option must take only one value");

            outputFilename = opt.OutputFilename[0];
        }

        // Parse frames option (may be multiple)
        IReadOnlyList<int>? frames;
        try
        {
            frames = ParseFramesOption(opt.Frames);
        }
        catch (ArgumentException ex)
        {
            return new ExtractOptions(cmd, error: ex.Message);
        }

        // Parse format option(s)
        List<DrawableFormater> frameFormatters;
        List<AnimationFormater> animationFormatters;
        try
        {
            (frameFormatters, animationFormatters) = ParseFormatOption(opt.FrameFormatTokens);
        }
        catch (ArgumentException ex)
        {
            return new ExtractOptions(cmd, error: ex.Message);
        }

        // If none given, default to SVG frame formatter
        if (frameFormatters.Count == 0 && animationFormatters.Count == 0)
            frameFormatters.Add(new DrawableFormater(ImageFormat.Svg));

        return new ExtractOptions(
            cmd,
            help: opt.Help,
            files: files,
            output: fullOutput,
            outputFilename: outputFilename,
            characters: opt.Characters.Select(v => Math.Max(v, 0)).ToArray(),
            exported: opt.Exported.ToArray(),
            frames: frames,
            fullAnimation: opt.FullAnimation,
            allSprites: opt.AllSprites,
            allExported: opt.AllExported,
            timeline: opt.Timeline,
            variables: opt.Variables,
            frameFormat: frameFormatters,
            animationFormat: animationFormatters
        );
    }

    // ---- Helpers (parsing) ----

    private sealed class ParsedOptions
    {
        public bool Help;
        public bool FullAnimation;
        public bool AllSprites;
        public bool AllExported;
        public bool Timeline;
        public bool Variables;

        public List<int> Characters { get; } = [];
        public List<string> Exported { get; } = [];
        public List<string> OutputFilename { get; } = [];    // expect 0..1 values
        public List<string> Frames { get; } = [];            // raw tokens (e.g., "1-3", "all")
        public List<string> FrameFormatTokens { get; } = []; // raw tokens (e.g., "png:a@800x600")
    }

    private static void ParseArgs(string[] argv, ParsedOptions opt, List<string> positionals)
    {
        // Recognized long options (with optional/required values)
        // --character, --exported, --output-filename, --frames, --frame-format expect values (can repeat)
        // Flags: --help, --all-sprites, --all-exported, --variables, --timeline, --full-animation
        var i = 0;
        bool afterDoubleDash = false;

        while (i < argv.Length)
        {
            var arg = argv[i];

            if (!afterDoubleDash && arg == "--")
            {
                afterDoubleDash = true;
                i++;
                continue;
            }

            if (!afterDoubleDash && arg.StartsWith("--", StringComparison.Ordinal))
            {
                var (name, maybeValue) = SplitLong(arg);
                switch (name)
                {
                    case "help":           opt.Help = true; break;
                    case "all-sprites":    opt.AllSprites = true; break;
                    case "all-exported":   opt.AllExported = true; break;
                    case "variables":      opt.Variables = true; break;
                    case "timeline":       opt.Timeline = true; break;
                    case "full-animation": opt.FullAnimation = true; break;

                    case "character":
                        AddValue(ref i, argv, maybeValue, v => opt.Characters.Add(ParseInt(v, "character")));
                        break;

                    case "exported":
                        AddValue(ref i, argv, maybeValue, v => opt.Exported.Add(v));
                        break;

                    case "output-filename":
                        AddValue(ref i, argv, maybeValue, v => opt.OutputFilename.Add(v));
                        break;

                    case "frames":
                        AddValue(ref i, argv, maybeValue, v => opt.Frames.Add(v));
                        break;

                    case "frame-format":
                        AddValue(ref i, argv, maybeValue, v => opt.FrameFormatTokens.Add(v));
                        break;

                    default:
                        // unknown long option → treat as positional
                        positionals.Add(arg);
                        break;
                }

                i++;
                continue;
            }

            if (!afterDoubleDash && arg.StartsWith("-", StringComparison.Ordinal) && arg.Length >= 2)
            {
                // Short options can be clustered, but only -h is valueless; -c/-e require a value
                // Handle character-by-character.
                var j = 1;
                while (j < arg.Length)
                {
                    var ch = arg[j];
                    switch (ch)
                    {
                        case 'h':
                            opt.Help = true;
                            j++;
                            break;

                        case 'c':
                        {
                            string? value;
                            if (j < arg.Length - 1)
                            {
                                // rest of same token
                                value = arg[(j + 1)..];
                                j = arg.Length;
                            }
                            else
                            {
                                // next argv
                                if (++i >= argv.Length)
                                    throw new ArgumentException("Option -c requires a value");
                                value = argv[i];
                                j = arg.Length;
                            }

                            opt.Characters.Add(ParseInt(value!, "c"));
                        }
                            break;

                        case 'e':
                        {
                            string? value;
                            if (j < arg.Length - 1)
                            {
                                value = arg[(j + 1)..];
                                j = arg.Length;
                            }
                            else
                            {
                                if (++i >= argv.Length)
                                    throw new ArgumentException("Option -e requires a value");
                                value = argv[i];
                                j = arg.Length;
                            }

                            opt.Exported.Add(value!);
                        }
                            break;

                        default:
                            // unknown short → push whole token as positional and stop scanning it
                            j = arg.Length;
                            positionals.Add(arg);
                            break;
                    }
                }

                i++;
                continue;
            }

            // Positional argument
            positionals.Add(arg);
            i++;
        }

        static (string name, string? value) SplitLong(string s)
        {
            var eq = s.IndexOf('=');
            if (eq >= 0)
                return (s.Substring(2, eq - 2), s[(eq + 1)..]);
            return (s[2..], null);
        }

        static void AddValue(ref int i, string[] argv, string? inlineValue, Action<string> add)
        {
            if (!string.IsNullOrEmpty(inlineValue))
            {
                add(inlineValue!);
            }
            else
            {
                if (++i >= argv.Length)
                    throw new ArgumentException("Missing value for option");
                add(argv[i]);
            }
        }

        static int ParseInt(string s, string optName)
        {
            if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                throw new ArgumentException($"Invalid integer for -{optName}/--{optName}: {s}");
            return v;
        }
    }

    /// <summary>
    /// Parse frames list like: "all", "3", "2-5", and allow multiple occurrences.
    /// Returns null for "all" (matches PHP behavior).
    /// </summary>
    private static IReadOnlyList<int>? ParseFramesOption(IEnumerable<string> raw)
    {
        var tokens = raw.ToList();
        if (tokens.Count == 0) return null;

        var frames = new List<int>();

        foreach (var token in tokens)
        {
            var text = (token ?? string.Empty).Trim();
            if (string.Equals(text, "all", StringComparison.OrdinalIgnoreCase))
                return null;

            var parts = text.Split('-', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 1)
            {
                frames.Add(Math.Max(ParsePosInt(parts[0]), 1));
            }
            else
            {
                var min = Math.Max(ParsePosInt(parts[0]), 1);
                var max = Math.Max(ParsePosInt(parts[1]), 1);
                if (max < min) (min, max) = (max, min);

                for (int v = min; v <= max; v++)
                    frames.Add(v);
            }
        }

        return frames;

        static int ParsePosInt(string s)
        {
            if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) || v <= 0)
                return Math.Max(v, 1);
            return v;
        }
    }

    /// <summary>
    /// Parse --frame-format values into Drawable/Animation formatters.
    /// Examples:
    ///   "png"               → frame PNG
    ///   "png:a"             → animated PNG flag (treated as animation → use GIF/WEBP in practice)
    ///   "webp:anim@800x600" → animated webp, resized to 800x600 (fit)
    ///   "jpeg@512x"         → jpeg, 512x512
    /// Flags before the format name (colon-separated): a|anim|animated|animation, plus arbitrary key[=value].
    /// </summary>
    private static (List<DrawableFormater> frames, List<AnimationFormater> anim)
        ParseFormatOption(IEnumerable<string> tokens)
    {
        var frameFormatters = new List<DrawableFormater>();
        var animationFormatters = new List<AnimationFormater>();

        foreach (var raw in tokens)
        {
            var token = (raw ?? string.Empty).Trim();
            if (token.Length == 0) continue;

            // Split on '@' → left: "flags:...:format", right(optional): "WxH" or "W"
            var atParts = token.Split('@', 2, StringSplitOptions.TrimEntries);
            var left = atParts[0].ToLowerInvariant();
            var sizePart = atParts.Length == 2 ? atParts[1] : null;

            // Split flags by ':'; last one is format name
            var colonParts = left.Split(':',
                                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (colonParts.Length == 0)
                throw new ArgumentException($"Invalid value for option --frame-format: {token}");

            var formatName = colonParts[^1];
            var flags = colonParts.Take(colonParts.Length - 1).ToArray();

            // Extract image options from flags
            var imageOptions = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in flags)
            {
                var kv = f.Split('=', 2, StringSplitOptions.TrimEntries);
                if (kv.Length == 1)
                    imageOptions[kv[0]] = true;
                else
                    imageOptions[kv[0]] = kv[1];
            }

            var isAnimation =
                imageOptions.ContainsKey("a") ||
                imageOptions.ContainsKey("anim") ||
                imageOptions.ContainsKey("animated") ||
                imageOptions.ContainsKey("animation");

            // Map format name to enum
            if (!TryMapImageFormat(formatName, out var fmt))
                throw new ArgumentException(
                    $"Invalid value for option --frame-format: the format {formatName} is not supported");

            // Parse @WxH (H optional -> square)
            IImageResizer? resizer = null;
            if (!string.IsNullOrEmpty(sizePart))
            {
                var dims = sizePart.Split('x', 2, StringSplitOptions.TrimEntries);
                var w = Math.Max(ParsePositiveInt(dims[0]), 1);
                var h = Math.Max(ParsePositiveInt(dims.Length == 2 ? dims[1] : dims[0]), 1);
                resizer = new FitSizeResizer(w, h);
            }

            if (isAnimation)
                animationFormatters.Add(new AnimationFormater(fmt, resizer, imageOptions));
            else
                frameFormatters.Add(new DrawableFormater(fmt, resizer, imageOptions));
        }

        return (frameFormatters, animationFormatters);

        static int ParsePositiveInt(string s)
        {
            if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) || v <= 0)
                return 1;
            return v;
        }

        static bool TryMapImageFormat(string name, out ImageFormat format)
        {
            switch (name.ToLowerInvariant())
            {
                case "svg":
                    format = ImageFormat.Svg;
                    return true;
                case "png":
                    format = ImageFormat.Png;
                    return true;
                case "jpeg":
                    format = ImageFormat.Jpeg;
                    return true;
                case "jpg":
                    format = ImageFormat.Jpeg;
                    return true;
                case "gif":
                    format = ImageFormat.Gif;
                    return true;
                case "webp":
                    format = ImageFormat.Webp;
                    return true;
                default:
                    format = default;
                    return false;
            }
        }
    }
}