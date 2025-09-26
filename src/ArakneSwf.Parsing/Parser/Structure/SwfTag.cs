using System.Diagnostics;
using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Parser.Structure;

/// <summary>
/// Structure for the tag before parsing.
/// To get the actual structure, call <see cref="Parse(SwfReader,int)"/>.
/// </summary>
public sealed class SwfTag
{
    /// <summary>
    /// Map of tag types that are definition tags (i.e., have a character id).
    /// </summary>
    public static readonly ISet<int> DefinitionTagTypes = new HashSet<int>
    {
        DefineShapeTag.TYPE_V1,
        DefineShapeTag.TYPE_V2,
        DefineShapeTag.TYPE_V3,
        DefineShape4Tag.TYPE_V4,
        DefineFontTag.TYPE_V1,
        DefineFont2Or3Tag.TypeV2,
        DefineFont2Or3Tag.TypeV3,
        DefineFont4Tag.TYPE_V4,
        DefineButtonTag.TYPE,
        DefineButton2Tag.TYPE,
        DefineSoundTag.TYPE,
        DefineSpriteTag.TYPE,
        DefineTextTag.TYPE_V1,
        DefineTextTag.TYPE_V2,
        DefineBitsLosslessTag.TYPE_V1,
        DefineBitsLosslessTag.TYPE_V2,
        DefineBitsTag.TYPE,
        DefineBitsJpeg2Tag.TYPE,
        DefineBitsJpeg3Tag.TYPE,
        DefineBitsJpeg4Tag.TYPE,
        DefineEditTextTag.TYPE,
        DefineMorphShapeTag.TYPE,
        DefineMorphShape2Tag.TYPE,
        DefineVideoStreamTag.TYPE,
        DefineBinaryDataTag.TYPE,
    };

    /// <summary>SWF tag type (spec value).</summary>
    public int Type { get; }

    /// <summary>Start of the tag data (after tag header), absolute byte offset.</summary>
    public int Offset { get; }

    /// <summary>Length of the tag data in bytes (excluding the tag header).</summary>
    public int Length { get; }

    /// <summary>Character ID for definition tags, otherwise null.</summary>
    public int? Id { get; }

    public SwfTag(int type, int offset, int length, int? id = null)
    {
        Debug.Assert(type >= 0);
        Debug.Assert(offset >= 0);
        Debug.Assert(length >= 0);

        Type = type;
        Offset = offset;
        Length = length;
        Id = id;
    }

    /// <summary>
    /// Parse the tag structure.
    /// Creates a chunked reader limited to the tag payload and dispatches to the concrete tag reader.
    /// </summary>
    /// <param name="reader">Base reader (not modified).</param>
    /// <param name="swfVersion">SWF file version.</param>
    /// <exception cref="ParserExceptionInterface"></exception>
    public object Parse(SwfReader reader, int swfVersion)
    {
        var bytePosEnd = Offset + Length;
        var r = reader.Chunk(Offset, bytePosEnd);

        if (bytePosEnd > r.End)
            bytePosEnd = r.End;

        object ret = Type switch
        {
            EndTag.TYPE                          => new EndTag(),
            ShowFrameTag.TYPE                    => new ShowFrameTag(),
            DefineShapeTag.TYPE_V1               => DefineShapeTag.Read(r, 1),
            PlaceObjectTag.TYPE                  => PlaceObjectTag.Read(r, bytePosEnd),
            RemoveObjectTag.TYPE                 => RemoveObjectTag.Read(r),
            DefineBitsTag.TYPE                   => DefineBitsTag.Read(r, bytePosEnd),
            DefineButtonTag.TYPE                 => DefineButtonTag.Read(r, bytePosEnd),
            JpegTablesTag.TYPE                   => JpegTablesTag.Read(r, bytePosEnd),
            SetBackgroundColorTag.TYPE           => SetBackgroundColorTag.Read(r),
            DefineFontTag.TYPE_V1                => DefineFontTag.Read(r),
            DefineTextTag.TYPE_V1                => DefineTextTag.Read(r, 1),
            DoActionTag.TYPE                     => DoActionTag.Read(r, bytePosEnd),
            DefineFontInfoTag.TYPE_V1            => DefineFontInfoTag.Read(r, 1, bytePosEnd),
            DefineSoundTag.TYPE                  => DefineSoundTag.Read(r, bytePosEnd),
            StartSoundTag.TYPE                   => StartSoundTag.Read(r),
            DefineButtonSoundTag.TYPE            => DefineButtonSoundTag.Read(r),
            SoundStreamHeadTag.TYPE_V1           => SoundStreamHeadTag.Read(r, 1),
            SoundStreamBlockTag.TYPE             => SoundStreamBlockTag.Read(r, bytePosEnd),
            DefineBitsLosslessTag.TYPE_V1        => DefineBitsLosslessTag.Read(r, 1, bytePosEnd),
            DefineBitsJpeg2Tag.TYPE              => DefineBitsJpeg2Tag.Read(r, bytePosEnd),
            DefineShapeTag.TYPE_V2               => DefineShapeTag.Read(r, 2),
            DefineButtonCxformTag.TYPE           => DefineButtonCxformTag.Read(r),
            ProtectTag.TYPE                      => ProtectTag.Read(r, bytePosEnd),
            PlaceObject2Tag.TYPE                 => PlaceObject2Tag.Read(r, swfVersion),
            RemoveObject2Tag.TYPE                => RemoveObject2Tag.Read(r),
            DefineShapeTag.TYPE_V3               => DefineShapeTag.Read(r, 3),
            DefineTextTag.TYPE_V2                => DefineTextTag.Read(r, 2),
            DefineButton2Tag.TYPE                => DefineButton2Tag.Read(r, bytePosEnd),
            DefineBitsJpeg3Tag.TYPE              => DefineBitsJpeg3Tag.Read(r, bytePosEnd),
            DefineBitsLosslessTag.TYPE_V2        => DefineBitsLosslessTag.Read(r, 2, bytePosEnd),
            DefineEditTextTag.TYPE               => DefineEditTextTag.Read(r),
            DefineSpriteTag.TYPE                 => DefineSpriteTag.Read(r, swfVersion, bytePosEnd),
            ProductInfo.TYPE                     => ProductInfo.Read(r),
            FrameLabelTag.TYPE                   => FrameLabelTag.Read(r, bytePosEnd),
            SoundStreamHeadTag.TYPE_V2           => SoundStreamHeadTag.Read(r, 2),
            DefineMorphShapeTag.TYPE             => DefineMorphShapeTag.Read(r),
            DefineFont2Or3Tag.TypeV2             => DefineFont2Or3Tag.Read(r, 2),
            ExportAssetsTag.ID                   => ExportAssetsTag.Read(r),
            ImportAssetsTag.TYPE_V1              => ImportAssetsTag.Read(r, 1),
            EnableDebuggerTag.TYPE_V1            => EnableDebuggerTag.Read(r, 1),
            DoInitActionTag.TYPE                 => DoInitActionTag.Read(r, bytePosEnd),
            DefineVideoStreamTag.TYPE            => DefineVideoStreamTag.Read(r),
            VideoFrameTag.TYPE                   => VideoFrameTag.Read(r, bytePosEnd),
            DefineFontInfoTag.TYPE_V2            => DefineFontInfoTag.Read(r, 2, bytePosEnd),
            EnableDebuggerTag.TYPE_V2            => EnableDebuggerTag.Read(r, 2),
            ScriptLimitsTag.TYPE                 => ScriptLimitsTag.Read(r),
            SetTabIndexTag.TYPE                  => SetTabIndexTag.Read(r),
            FileAttributesTag.TYPE               => FileAttributesTag.Read(r),
            PlaceObject3Tag.TYPE                 => PlaceObject3Tag.Read(r, swfVersion),
            ImportAssetsTag.TYPE_V2              => ImportAssetsTag.Read(r, 2),
            DefineFontAlignZonesTag.TYPE         => DefineFontAlignZonesTag.Read(r, bytePosEnd),
            CsmTextSettingsTag.ID                => CsmTextSettingsTag.Read(r),
            DefineFont2Or3Tag.TypeV3             => DefineFont2Or3Tag.Read(r, 3),
            SymbolClassTag.TYPE                  => SymbolClassTag.Read(r),
            MetadataTag.TYPE                     => MetadataTag.Read(r),
            DefineScalingGridTag.TYPE            => DefineScalingGridTag.Read(r),
            DoAbcTag.TYPE                        => DoAbcTag.Read(r, bytePosEnd),
            DefineShape4Tag.TYPE_V4              => DefineShape4Tag.Read(r),
            DefineMorphShape2Tag.TYPE            => DefineMorphShape2Tag.Read(r),
            DefineSceneAndFrameLabelDataTag.TYPE => DefineSceneAndFrameLabelDataTag.Read(r),
            DefineBinaryDataTag.TYPE             => DefineBinaryDataTag.Read(r, bytePosEnd),
            DefineFontNameTag.TYPE               => DefineFontNameTag.Read(r),
            StartSound2Tag.TYPE                  => StartSound2Tag.Read(r),
            DefineBitsJpeg4Tag.TYPE              => DefineBitsJpeg4Tag.Read(r, bytePosEnd),
            DefineFont4Tag.TYPE_V4               => DefineFont4Tag.Read(r, bytePosEnd),
            ReflexTag.TYPE                       => ReflexTag.Read(r, bytePosEnd),
            _                                    => UnknownTag.Create(r, Type, bytePosEnd),
        };

        // Extra data after parsing the tag payload?
        if (r.Offset < bytePosEnd && (r.Errors & Errors.ExtraData) != 0)
        {
            var len = bytePosEnd - r.Offset;
            Debug.Assert(len > 0);

            throw new ParserExtraDataException(
                $"Extra data found after tag {Type} at offset {r.Offset} (length = {len})",
                r.Offset,
                maxAllowedLength: null,
                actualLength: len
            );
        }

        return ret;
    }

    /// <summary>
    /// Read all tags from the stream (until <paramref name="end"/> or reader end).
    /// </summary>
    /// <param name="reader">Reader positioned on the first tag.</param>
    /// <param name="end">Optional absolute end offset; if null, uses reader.End.</param>
    /// <param name="parseId">If true, parse and expose character ID for definition tags.</param>
    public static IEnumerable<SwfTag> ReadAll(SwfReader reader, int? end = null, bool parseId = true)
    {
        var limit = end ?? reader.End;

        while (reader.Offset < limit)
        {
            var recordHeader = reader.ReadUi16();
            var tagType = recordHeader >> 6;
            var tagLength = recordHeader & 0x3F;

            if (tagLength == 0x3F)
            {
                // long form
                tagLength = checked((int)reader.ReadUi32());
            }

            if (parseId && IsDefinitionTagType(tagType) && tagLength >= 2)
            {
                // next two bytes are the character ID
                int id = reader.ReadUi16();
                yield return new SwfTag(type: tagType, offset: reader.Offset - 2, length: tagLength, id: id);
                reader.SkipBytes(tagLength - 2); // 2 bytes already consumed
            }
            else
            {
                yield return new SwfTag(type: tagType, offset: reader.Offset, length: tagLength);
                reader.SkipBytes(tagLength);
            }
        }
    }

    /// <summary>Returns true if <paramref name="type"/> is a definition tag (has a character id).</summary>
    private static bool IsDefinitionTagType(int type) => DefinitionTagTypes.Contains(type);
}