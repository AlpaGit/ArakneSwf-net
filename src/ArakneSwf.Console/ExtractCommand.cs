using System.Text;
using System.Text.Json;
using ArakneSwf.Parsing;
using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Extractor;
using ArakneSwf.Parsing.Extractor.Images;
using ArakneSwf.Parsing.Extractor.Shapes;
using ArakneSwf.Parsing.Extractor.Sprite;
using ArakneSwf.Parsing.Extractor.Timelines;
using ArakneSwf.Parsing.Parser.Structure.Record;
using Path = System.IO.Path;

namespace SwfRender.Console;

/// <summary>
/// Console command to extract resources from SWF files.
/// </summary>
public sealed class ExtractCommand
{
    public int Execute(string[] args, ExtractOptions? options = null)
    {
        options ??= ExtractOptions.CreateFromCli(args);

        if (options.Help)
        {
            Usage(options);
            return 0;
        }

        if (!string.IsNullOrEmpty(options.Error))
        {
            Usage(options, options.Error);
            return 1;
        }

        var count = options.Files.Count;
        var success = true;

        for (int i = 0; i < count; i++)
        {
            var file = options.Files[i];
            System.Console.Write($"[{i + 1}/{count}] Processing file: {file} ");

            try
            {
                if (Process(options, file))
                {
                    System.Console.WriteLine("done");
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"error: {e}");
                success = false;
            }
        }

        if (!success)
        {
            System.Console.WriteLine("Some errors occurred during the extraction process.");
            return 1;
        }

        System.Console.WriteLine("All files processed successfully.");
        return 0;
    }

    public void Usage(ExtractOptions options, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
        {
            System.Console.WriteLine($"Error: {error}");
            System.Console.WriteLine();
        }

        // NOTE: Double the braces to display {placeholders} literally in an interpolated string.
        var help = $$"""

                     Extract resources from an SWF file.

                     Usage:
                         {{options.Command}} [options] <file> [<file> ...] <output>

                     Options:
                         -h, --help            Show this help message
                         -c, --character <id>  Specify the character id to extract. This option is repeatable.
                         -e, --exported <name> Extract the character with the specified exported name. This option is repeatable.
                         --frames <frames>     Frames to export, if applicable. Can be a single frame number, a range (e.g. 1-10), or "all".
                                               By default, all frames will be exported. This option is repeatable.
                         --full-animation      Extract the full animation for animated characters.
                                               If set, the frames count will be computed on included sprites, instead of counting
                                               only the current character.
                         --variables           Extract action script variables to JSON
                         --all-sprites         Extract all sprites from the SWF file
                         --all-exported        Extract all exported symbols from the SWF file
                         --timeline            Extract the root SWF animation
                         --output-filename     Define the filename pattern to use for the output files
                                               (default: {{options.OutputFilename}})
                                               Takes the following placeholders:
                                               - {basename}: The base name of the SWF file
                                               - {name}: The name or id of the character / exported symbol
                                               - {ext}: The file extension (png, svg, json, etc.)
                                               - {frame}/{_frame}: The frame number (1-based). {_frame} will prefix with "_" if needed
                                               - {dirname}: The name of the directory containing the SWF file
                         --frame-format <format>
                                               Specify the format to use for the sprite or timeline frames. This option is repeatable.
                                               The format is <options>:<filetype>@<width>x<height>, where:
                                               - <options> are optional options to apply to the output, separated by ":"
                                               - <filetype> is the type of file to generate (svg, png, gif, webp).
                                                   When an animated file is requested, all frames will be exported, even if the --frames option is used.
                                               - <width> is the width of the output image (optional).
                                               - <height> is the height of the output image (optional).
                                                   If only the width is specified, the height will be set to the same value.
                                               Availables options:
                                               - a/anim/animated: (gif, webp) Export the frames as an animated file.
                                                                  When is requested, all frames will be exported, even if the --frames option is used.
                                               - lossless: (webp) Use lossless compression for the output image.
                                               - quality=<number>: (webp, jpeg) Set the quality (i.e. lossy compression) of the output image (0-100).
                                               - compression=<number>: (png, webp) Set the compression level of the output image (0-6 for webp, 0-9 for png).
                                               - format=<format>: (png) Set the PNG format to use (e.g. png8, png24, png32).
                                               - bit-depth=<number>: (png) Set the bit depth of the output image.
                                               - sampling=<string>: (jpeg) Set the sampling factor for the JPEG image (e.g. 420, 422, 444).
                                               - size=<string>: (jpeg) Set the maximum file size for the JPEG image (e.g. 100k, 1M).
                                               - loop=<number>: (gif) Set the number of loops for the GIF animation (0 for infinite loop).


                     Arguments:
                         <file>      The SWF file to extract resources from. Multiple files can be specified.
                         <output>    The output directory where the extracted resources will be saved.

                     Examples:
                         Extract all exported symbols from a SWF file
                             {{options.Command}} --all-exported --output-filename myfile.swf export

                         Extract the first frame of the main timeline of a SWF file as PNG of size 128x128
                             {{options.Command}} --timeline --frames 1 --frame-format png@128x128 --output-filename {basename}.{ext} myfile.swf export

                         Extract a single sprite animation, with all its sub-animations
                             {{options.Command}} -e myAnim --full-animation --output-filename {basename}/{frame}.{ext} myfile.swf export

                         Extract an animation as lossless animated webp
                             {{options.Command}} -e myAnim --full-animation --frame-format a:lossless:webp --output-filename {basename}/{frame}.{ext} myfile.swf export

                         Extract a character as jpeg with quality 80 and 4:2:0 sampling factors
                             {{options.Command}} -c 123 --frame-format quality=80:sampling=420:jpeg myfile.swf export

                     """;
        System.Console.WriteLine(help);
    }

    public bool Process(ExtractOptions options, string file)
    {
        var swf = new SwfFile(file,
                              errors: Errors.IgnoreInvalidTag & ~Errors.ExtraData & ~Errors.UnprocessableData);

        if (!swf.Valid())
        {
            System.Console.WriteLine($"error: The file {file} is not a valid SWF file");
            return false;
        }

        var extractor = new SwfExtractor(swf);
        var success = true;

        try
        {
            foreach (var characterId in options.Characters)
            {
                success = ProcessCharacter(options, swf, characterId.ToString(), extractor.Character(characterId)) &&
                          success;
            }

            foreach (var name in options.Exported)
            {
                try
                {
                    var character = extractor.ByName(name);
                    success = ProcessCharacter(options, swf, name, character) && success;
                }
                catch (ArgumentException)
                {
                    System.Console.WriteLine($"The character {name} is not exported in the SWF file");
                    success = false;
                }
            }

            if (options.AllSprites)
            {
                foreach (var kv in extractor.Sprites())
                {
                    var id = kv.Key;
                    var sprite = kv.Value;
                    success = ProcessCharacter(options, swf, id.ToString(), sprite) && success;
                }
            }

            if (options.AllExported)
            {
                foreach (var kv in extractor.Exported())
                {
                    var name = kv.Key;
                    var id = kv.Value;
                    var character = extractor.Character(id);
                    success = ProcessCharacter(options, swf, name, character) && success;
                }
            }

            if (options.Timeline)
            {
                success = ProcessCharacter(options, swf, "timeline", extractor.Timeline(false)) && success;
            }

            if (options.Variables)
            {
                success = ProcessVariables(options, "variables", swf) && success;
            }
        }
        finally
        {
            extractor.Release();
        }

        return success;
    }

    private bool ProcessCharacter(
        ExtractOptions options,
        SwfFile        file,
        string         name,
        object         character)
    {
        try
        {
            switch (character)
            {
                case Timeline timeline:
                    return ProcessTimeline(options, file, name, timeline);

                case SpriteDefinition sprite:
                    return ProcessSprite(options, file, name, sprite);

                case IImageCharacter imageChar:
                    return ProcessImage(options, file.Path, name, imageChar);

                case ShapeDefinition shape:
                    return ProcessShape(options, file.Path, name, shape);

                case MissingCharacter:
                    System.Console.WriteLine($"The character {name} is missing in the SWF file or unsupported");
                    return false;

                default:
                    System.Console.WriteLine($"Unsupported character type for {name}");
                    return false;
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine($"An error occurred while processing the character {name}: {e.Message}");
            return false;
        }
    }

    private bool ProcessVariables(ExtractOptions options, string name, SwfFile swf)
    {
        var variables = swf.Variables();

        var json = JsonSerializer.Serialize(
            variables,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

        return WriteToOutputDir(json, swf.Path, options, name, "json");
    }

    private bool ProcessTimeline(ExtractOptions options, SwfFile file, string name, Timeline timeline)
    {
        var success = true;

        // Animated outputs
        foreach (var formatter in options.AnimationFormat)
        {
            var bytes = formatter.DoFormat(timeline, file.FrameRate(), options.FullAnimation);
            success = WriteToOutputDir(bytes, file.Path, options, name, formatter.Extension()) && success;
        }

        var framesCount = timeline.FramesCount(options.FullAnimation);

        if (framesCount == 1)
            return ProcessTimelineFrame(options, file.Path, name, timeline) && success;

        if (options.Frames is null)
        {
            for (int frame = 0; frame < framesCount; frame++)
            {
                success = ProcessTimelineFrame(options, file.Path, name, timeline, frame) && success;
            }

            return success;
        }

        foreach (var f in options.Frames)
        {
            if (f > framesCount)
                break;

            success = ProcessTimelineFrame(options, file.Path, name, timeline, f - 1) && success;
        }

        return success;
    }

    /// <summary>
    /// Process a single timeline frame.
    /// </summary>
    private bool ProcessTimelineFrame(
        ExtractOptions options,
        string         file,
        string         name,
        Timeline       timeline,
        int?           frame = null)
    {
        var success = true;

        foreach (var formatter in options.FrameFormat)
        {
            var bytes = formatter.FormatDrawable(timeline, frame ?? 0);
            var wrote = WriteToOutputDir(bytes,
                                         file,
                                         options,
                                         name,
                                         formatter.Extension(),
                                         frame != null ? frame + 1 : (int?)null);
            success = wrote && success;
        }

        return success;
    }

    private bool ProcessImage(ExtractOptions options, string file, string name, IImageCharacter image)
    {
        var best = image.ToBestFormat(); // expected: object with .Data (byte[]) and .Type.Extension() (string)
        return WriteToOutputDir(best.Data, file, options, name, best.Type.Extension());
    }

    private bool ProcessSprite(ExtractOptions options, SwfFile file, string name, SpriteDefinition sprite)
    {
        return ProcessTimeline(options, file, name, sprite.DoTimeline());
    }

    private bool ProcessShape(ExtractOptions options, string file, string name, ShapeDefinition shape)
    {
        var svg = shape.ToSvg(); // string
        return WriteToOutputDir(svg, file, options, name, "svg");
    }

    // -------- output helpers --------

    private bool WriteToOutputDir(string content, string file, ExtractOptions options, string name, string ext,
                                  int?   frame = null)
    {
        var outputFile = BuildOutputPath(options, file, name, ext, frame);

      /*  if (File.Exists(outputFile))
        {
            System.Console.WriteLine($"The file {outputFile} already exists, skipping");
            return false;
        }
*/
        var dir = Path.GetDirectoryName(outputFile);
        try
        {
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        catch
        {
            System.Console.WriteLine($"Cannot create output directory: {dir}");
            return false;
        }

        try
        {
            File.WriteAllText(outputFile, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return true;
        }
        catch
        {
            System.Console.WriteLine($"Cannot write to output file: {outputFile}");
            return false;
        }
    }

    private bool WriteToOutputDir(byte[] content, string file, ExtractOptions options, string name, string ext,
                                  int?   frame = null)
    {
        var outputFile = BuildOutputPath(options, file, name, ext, frame);

        /*if (File.Exists(outputFile))
        {
            System.Console.WriteLine($"The file {outputFile} already exists, skipping");
            return false;
        }*/

        var dir = Path.GetDirectoryName(outputFile);
        try
        {
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        catch
        {
            System.Console.WriteLine($"Cannot create output directory: {dir}");
            return false;
        }

        try
        {
            File.WriteAllBytes(outputFile, content);
            return true;
        }
        catch
        {
            System.Console.WriteLine($"Cannot write to output file: {outputFile}");
            return false;
        }
    }

    private static string BuildOutputPath(ExtractOptions options, string file, string name, string ext, int? frame)
    {
        var swfBase = Path.GetFileNameWithoutExtension(file);
        var dirName =
            Path.GetFileName(Path.GetDirectoryName(file)
                                 ?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) ??
                             string.Empty);

        var map = new Dictionary<string, string>
        {
            ["{basename}"] = swfBase,
            ["{name}"] = name,
            ["{ext}"] = ext,
            ["{frame}"] = frame.HasValue ? frame.Value.ToString()! : string.Empty,
            ["{_frame}"] = frame.HasValue ? "_" + frame.Value.ToString() : string.Empty,
            ["{dirname}"] = dirName,
        };

        var fileName = options.OutputFilename;
        foreach (var kv in map)
            fileName = fileName.Replace(kv.Key, kv.Value, StringComparison.Ordinal);

        return Path.Combine(options.Output, fileName);
    }
}