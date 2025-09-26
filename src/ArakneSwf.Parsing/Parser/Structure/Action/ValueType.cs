namespace ArakneSwf.Parsing.Parser.Structure.Action;

/// <summary>Type d’élément utilisé (p.ex. dans ActionPush).</summary>
public enum ValueType : byte
{
    /// <summary>Null-terminated string</summary>
    String = 0,

    /// <summary>32 bits float</summary>
    Float = 1,

    /// <summary>null</summary>
    Null = 2,

    /// <summary>undefined</summary>
    Undefined = 3,

    /// <summary>Register number (unsigned 8 bits)</summary>
    Register = 4,

    /// <summary>boolean</summary>
    Boolean = 5,

    /// <summary>64 bits float</summary>
    Double = 6,

    /// <summary>32 bits signed integer</summary>
    Integer = 7,

    /// <summary>8-bit constant id (reference to constant pool)</summary>
    Constant8 = 8,

    /// <summary>16-bit constant id (reference to constant pool)</summary>
    Constant16 = 9
}