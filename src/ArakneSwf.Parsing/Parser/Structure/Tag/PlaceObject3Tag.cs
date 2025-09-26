// This file is part of Arakne-Swf.
// GNU LGPL v3-or-later. See <https://www.gnu.org/licenses/>

using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Filters;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// PlaceObject3 tag (TYPE = 70).
/// </summary>
public sealed class PlaceObject3Tag
{
    public const int TYPE = 70;

    /// <inheritdoc cref="PlaceObject2Tag.Move"/>
    public bool Move { get; }

    /// <summary>Introduced in PlaceObject3.</summary>
    public bool HasImage { get; }

    /// <inheritdoc cref="PlaceObject2Tag.Depth"/>
    public int Depth { get; }

    /// <summary>Introduced in PlaceObject3.</summary>
    public string? ClassName { get; }

    /// <inheritdoc cref="PlaceObject2Tag.CharacterId"/>
    public int? CharacterId { get; }

    /// <inheritdoc cref="PlaceObject2Tag.Matrix"/>
    public Matrix? Matrix { get; }

    /// <inheritdoc cref="PlaceObject2Tag.ColorTransform"/>
    public ColorTransform? ColorTransform { get; }

    /// <inheritdoc cref="PlaceObject2Tag.Ratio"/>
    public int? Ratio { get; }

    /// <inheritdoc cref="PlaceObject2Tag.Name"/>
    public string? Name { get; }

    /// <inheritdoc cref="PlaceObject2Tag.ClipDepth"/>
    public int? ClipDepth { get; }

    /// <summary>Introduced in PlaceObject3.</summary>
    public List<Filter>? SurfaceFilterList { get; }

    /// <summary>Introduced in PlaceObject3.</summary>
    public int? BlendMode { get; }

    /// <summary>Introduced in PlaceObject3.</summary>
    public int? BitmapCache { get; }

    /// <inheritdoc cref="PlaceObject2Tag.ClipActions"/>
    public ClipActions? ClipActions { get; }

    public PlaceObject3Tag(
        bool            move,
        bool            hasImage,
        int             depth,
        string?         className,
        int?            characterId,
        Matrix?         matrix,
        ColorTransform? colorTransform,
        int?            ratio,
        string?         name,
        int?            clipDepth,
        List<Filter>?   surfaceFilterList,
        int?            blendMode,
        int?            bitmapCache,
        ClipActions?    clipActions)
    {
        Move = move;
        HasImage = hasImage;
        Depth = depth;
        ClassName = className;
        CharacterId = characterId;
        Matrix = matrix;
        ColorTransform = colorTransform;
        Ratio = ratio;
        Name = name;
        ClipDepth = clipDepth;
        SurfaceFilterList = surfaceFilterList;
        BlendMode = blendMode;
        BitmapCache = bitmapCache;
        ClipActions = clipActions;
    }

    /// <summary>
    /// Read a PlaceObject3 tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="swfVersion">SWF version of the file being read.</param>
    public static PlaceObject3Tag Read(SwfReader reader, int swfVersion)
    {
        int flags = reader.ReadUi8();
        var placeFlagHasClipActions = (flags & 0b1000_0000) != 0;
        var placeFlagHasClipDepth = (flags & 0b0100_0000) != 0;
        var placeFlagHasName = (flags & 0b0010_0000) != 0;
        var placeFlagHasRatio = (flags & 0b0001_0000) != 0;
        var placeFlagHasColorTransform = (flags & 0b0000_1000) != 0;
        var placeFlagHasMatrix = (flags & 0b0000_0100) != 0;
        var placeFlagHasCharacter = (flags & 0b0000_0010) != 0;
        var placeFlagMove = (flags & 0b0000_0001) != 0;

        flags = reader.ReadUi8();
        // 3 bits reserved (must be 0)
        var placeFlagHasImage = (flags & 0b0001_0000) != 0;
        var placeFlagHasClassName = (flags & 0b0000_1000) != 0;
        var placeFlagHasCacheAsBitmap = (flags & 0b0000_0100) != 0;
        var placeFlagHasBlendMode = (flags & 0b0000_0010) != 0;
        var placeFlagHasFilterList = (flags & 0b0000_0001) != 0;

        int depth = reader.ReadUi16();
        var className = (placeFlagHasClassName || (placeFlagHasImage && placeFlagHasCharacter))
            ? reader.ReadNullTerminatedString()
            : null;

        var characterId = placeFlagHasCharacter ? reader.ReadUi16() : (int?)null;
        var matrix = placeFlagHasMatrix ? Matrix.Read(reader) : null;
        var cxform = placeFlagHasColorTransform ? ColorTransform.Read(reader, withAlpha: true) : null;
        var ratio = placeFlagHasRatio ? reader.ReadUi16() : (int?)null;
        var name = placeFlagHasName ? reader.ReadNullTerminatedString() : null;
        var clipDepth = placeFlagHasClipDepth ? reader.ReadUi16() : (int?)null;
        var filters = placeFlagHasFilterList ? Filter.ReadCollection(reader) : null;
        var blendMode = placeFlagHasBlendMode ? reader.ReadUi8() : (int?)null;
        var bitmapCache = placeFlagHasCacheAsBitmap ? reader.ReadUi8() : (int?)null;
        var clipActions = placeFlagHasClipActions ? ClipActions.Read(reader, swfVersion) : null;

        return new PlaceObject3Tag(
            move: placeFlagMove,
            hasImage: placeFlagHasImage,
            depth: depth,
            className: className,
            characterId: characterId,
            matrix: matrix,
            colorTransform: cxform,
            ratio: ratio,
            name: name,
            clipDepth: clipDepth,
            surfaceFilterList: filters,
            blendMode: blendMode,
            bitmapCache: bitmapCache,
            clipActions: clipActions
        );
    }
}
