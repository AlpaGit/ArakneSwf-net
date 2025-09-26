using System.Diagnostics;
using System.Text;
using ImageMagick;

namespace ArakneSwf.Parsing.Extractor.Drawer.Converters.Renderer;

/// <summary>
/// Base type for command-based SVG renderers using UNIX pipes.
/// Spawns a CLI, writes SVG to stdin, reads raster bytes from stdout.
/// </summary>
public abstract class AbstractCommandImagickSvgRenderer : IImagickSvgRenderer
{
    protected readonly string Command;

    protected AbstractCommandImagickSvgRenderer(string command = "")
    {
        Command = command ?? string.Empty;
    }

    /// <summary>
    /// Open/rasterize the given SVG using the configured command.
    /// </summary>
    public MagickImage Open(string svg, string backgroundColor)
    {
        if (svg is null) throw new ArgumentNullException(nameof(svg));

        // Let the subclass build a full shell command (responsible for escaping args).
        var shellCommand = BuildCommand(Command, backgroundColor);

        // Run via /bin/sh -lc "<cmd>" to support pipes, redirects, etc.
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/sh",
            ArgumentList = { "-lc", shellCommand },
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var proc = new Process { StartInfo = psi };
        if (!proc.Start())
            throw new InvalidOperationException("Failed to start SVG renderer process.");

        // Write SVG to stdin (UTF-8)
        using (var stdin = new StreamWriter(proc.StandardInput.BaseStream, new UTF8Encoding(false)))
        {
            stdin.Write(svg);
        }

        // Read stdout to bytes
        byte[] pngBytes;
        using (var ms = new MemoryStream())
        {
            proc.StandardOutput.BaseStream.CopyTo(ms);
            pngBytes = ms.ToArray();
        }

        // If nothing came out, grab stderr and fail
        if (pngBytes.Length == 0)
        {
            var err = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            throw new InvalidOperationException("SVG conversion failed: " + err);
        }

        proc.WaitForExit();

        var img = new MagickImage(pngBytes);
        // Background can matter if you later flatten or write formats without alpha.
        img.BackgroundColor = new MagickColor(backgroundColor);
        return img;
    }

    /// <summary>
    /// Check if the command is available on this system (UNIX).
    /// </summary>
    public virtual bool Supported()
    {
        if (string.IsNullOrWhiteSpace(Command)) return false;

        var whichCmd = $"/usr/bin/which {EscapeShellArg(Command)} 2>/dev/null";
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/sh",
            ArgumentList = { "-lc", whichCmd },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        try
        {
            using var p = Process.Start(psi)!;
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return !string.IsNullOrWhiteSpace(output);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Build the CLI command to execute.
    /// Implementations must return a complete shell command string.
    /// NOTE: You should escape <paramref name="backgroundColor"/> within this method.
    /// </summary>
    protected abstract string BuildCommand(string command, string backgroundColor);

    protected static string EscapeShellArg(string? s)
    {
        // POSIX single-quote escape: ' -> '"'"'
        if (s is null) return "''";
        return "'" + s.Replace("'", "'\"'\"'") + "'";
    }
}