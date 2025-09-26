namespace ArakneSwf.Parsing.Avm;

public enum MultinameKind : int
{
    /// <summary>Qualified name kind</summary>
    QName = 7,

    /// <summary>Qualified name of attribute kind</summary>
    QNameA = 0x0D,

    /// <summary>Runtime qualified name kind</summary>
    RtqName = 0x0F,

    /// <summary>Runtime qualified name of attribute kind</summary>
    RtqNameA = 0x10,

    /// <summary>Runtime qualified name with late resolution kind</summary>
    RtqNameL = 0x11,

    /// <summary>Runtime qualified name of attribute with late resolution kind</summary>
    RtqNameLA = 0x12,

    /// <summary>Multiname kind</summary>
    MultiName = 0x09,

    /// <summary>Multiname of attribute kind</summary>
    MultiNameA = 0x0E,

    /// <summary>Multiname with late resolution kind</summary>
    MultiNameL = 0x1B,

    /// <summary>Multiname of attribute with late resolution kind</summary>
    MultiNameLA = 0x1C,

    /// <summary>Type name kind</summary>
    TypeName = 0x1D
}

