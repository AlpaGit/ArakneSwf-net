using ArakneSwf.Parsing.Avm;

namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// DoABC tag (TYPE = 82).
/// </summary>
public sealed class DoAbcTag
{
    private ushort _minorVersion;
    private ushort _majorVersion;
    public const int TYPE = 82;

    public const int MinorWithDecimal = 17;

    public int Flags { get; }
    public string Name { get; }

    /// <summary>Raw ABC bytecode data.</summary>
    public byte[] Data { get; }

    public DoAbcTag(int flags, string name, byte[] data)
    {
        Flags = flags;
        Name = name;
        Data = data;
    }

    /// <summary>
    /// Read a DoABC tag from the SWF reader.
    /// </summary>
    /// <param name="reader">Reader positioned at the start of the tag body.</param>
    /// <param name="end">End byte offset (exclusive) of this tag's data.</param>
    public static DoAbcTag Read(SwfReader reader, int end)
    {
        var flags = reader.ReadUi32();
        string name = reader.ReadNullTerminatedString();
        var data = reader.ReadBytesTo(end);

        var tag = new DoAbcTag((int)flags, name, data);
        tag.ReadFull(new SwfReader(data));
        return tag;
    }

    public void ReadFull(SwfReader reader)
    {
        _minorVersion = reader.ReadUi16();
        _majorVersion = reader.ReadUi16();

        // constant integers
        var constantIntPoolCount = reader.ReadU30();
        if (constantIntPoolCount > 1)
        {
            for (var i = 1; i < constantIntPoolCount; i++)
            {
                var constant = reader.ReadSi32();
            }
        }

        // constant unsigned integers
        var constantUIntPoolCount = reader.ReadU30();
        if (constantUIntPoolCount > 1)
        {
            for (var i = 1; i < constantUIntPoolCount; i++)
            {
                var constant = reader.ReadUi32();
            }
        }

        // constant double
        var constantDoublePoolCount = reader.ReadU30();
        if (constantDoublePoolCount > 1)
        {
            for (var i = 1; i < constantDoublePoolCount; i++)
            {
                var constant = reader.ReadDouble();
            }
        }

        if (HasDecimalSupport())
        {
            // constant decimal
            var constantDecimalPoolCount = reader.ReadU30();
            if (constantDecimalPoolCount > 1)
            {
                for (var i = 1; i < constantDecimalPoolCount; i++)
                {
                    var constant = reader.ReadDecimal();
                }
            }
        }

        // constant float
        if (HasFloatSupport())
        {
            var constantFloatPoolCount = reader.ReadU30();
            if (constantFloatPoolCount > 1)
            {
                for (var i = 1; i < constantFloatPoolCount; i++)
                {
                    var constant = reader.ReadFloat();
                }
            }
        }

        // constant string
        var constantStringPoolCount = reader.ReadU30();
        if (constantStringPoolCount > 1)
        {
            for (var i = 1; i < constantStringPoolCount; i++)
            {
                var constant = reader.ReadString();
            }
        }

        // constant namespace
        var constantNamespacePoolCount = reader.ReadU30();
        if (constantNamespacePoolCount > 1)
        {
            for (var i = 1; i < constantNamespacePoolCount; i++)
            {
                var kind = reader.ReadUi8();
                var nameIndex = reader.ReadU30();
            }
        }

        // constant namespace set
        var constantNamespaceSetPoolCount = reader.ReadU30();
        if (constantNamespaceSetPoolCount > 1)
        {
            for (var i = 1; i < constantNamespaceSetPoolCount; i++)
            {
                var count = reader.ReadU30();
                for (var j = 0; j < count; j++)
                {
                    var nsIndex = reader.ReadU30();
                }
            }
        }

        // constant multiname
        var constantMultinamePoolCount = reader.ReadU30();
        if (constantMultinamePoolCount > 1)
        {
            for (var i = 1; i < constantMultinamePoolCount; i++)
            {
                var kind = reader.ReadUi8();

                switch ((MultinameKind)kind)
                {
                    case MultinameKind.QName:
                    case MultinameKind.QNameA:
                    {
                        var nsIndex = reader.ReadU30();
                        var nameIndex = reader.ReadU30();
                        break;
                    }
                    case MultinameKind.RtqName:
                    case MultinameKind.RtqNameA:
                    {
                        var nameIndex = reader.ReadU30();
                        break;
                    }
                    case MultinameKind.RtqNameL:
                    case MultinameKind.RtqNameLA:
                    {
                        break;
                    }
                    case MultinameKind.MultiName:
                    case MultinameKind.MultiNameA:
                    {
                        var nameIndex = reader.ReadU30();
                        var nsSetIndex = reader.ReadU30();
                        break;
                    }
                    case MultinameKind.MultiNameL:
                    case MultinameKind.MultiNameLA:
                    {
                        var nsSetIndex = reader.ReadU30();
                        break;
                    }
                    case MultinameKind.TypeName:
                    {
                        var nameIndex = reader.ReadU30();
                        var paramCount = reader.ReadU30();
                        for (var j = 0; j < paramCount; j++)
                        {
                            var paramIndex = reader.ReadU30();
                        }

                        break;
                    }
                }
            }
        }

        var methodCount = reader.ReadU30();
        for (var i = 0; i < methodCount; i++)
        {
            var paramCount = reader.ReadU30();
            var returnTypeIndex = reader.ReadU30();
            for (var j = 0; j < paramCount; j++)
            {
                var paramTypeIndex = reader.ReadU30();
            }

            var nameIndex = reader.ReadU30();
            var flags = reader.ReadUi8();

            if ((flags & 0x08) != 0) // HAS_OPTIONAL
            {
                var optionalCount = reader.ReadU30();
                for (var j = 0; j < optionalCount; j++)
                {
                    var valIndex = reader.ReadU30();
                    var kind = reader.ReadUi8();
                }
            }

            if ((flags & 0x80) != 0) // HAS_PARAM_NAMES
            {
                for (var j = 0; j < paramCount; j++)
                {
                    var paramNameIndex = reader.ReadU30();
                }
            }
        }

        var classCount = reader.ReadU30();
        for (var i = 0; i < classCount; i++)
        {
            var nameIndex = reader.ReadU30();
            var superNameIndex = reader.ReadU30();
            var flags = reader.ReadUi8();
            if ((flags & 0x08) != 0) // CLASS_FLAG_protectedNs
            {
                var protectedNsIndex = reader.ReadU30();
            }
            var interfaceCount = reader.ReadU30();
            for (var j = 0; j < interfaceCount; j++)
            {
                var interfaceIndex = reader.ReadU30();
            }
            var traitCount = reader.ReadU30();
            for (var j = 0; j < traitCount; j++)
            {
                ReadTrait(reader);
            }
        }
    }

    private void ReadTrait(SwfReader reader)
    {
    }

    public bool HasDecimalSupport()
    {
        return _minorVersion >= MinorWithDecimal;
    }

    public bool HasFloatSupport()
    {
        return _majorVersion >= 47 && _minorVersion >= 16;
    }
}