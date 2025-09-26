namespace ArakneSwf.Parsing.Parser.Structure.Tag;

/// <summary>
/// SetTabIndex (TYPE = 66)
/// </summary>
public sealed class SetTabIndexTag
{
    public const int TYPE = 66;

    /// <summary>
    /// Profondeur d'affichage (UI16, non-négatif).
    /// </summary>
    public int Depth { get; }

    /// <summary>
    /// Index d'onglet (UI16, non-négatif).
    /// </summary>
    public int TabIndex { get; }

    public SetTabIndexTag(int depth, int tabIndex)
    {
        Depth = depth;
        TabIndex = tabIndex;
    }

    /// <summary>
    /// Lit un tag SetTabIndex depuis le lecteur SWF.
    /// </summary>
    /// <param name="reader">Lecteur binaire SWF.</param>
    /// <returns>Instance de <see cref="SetTabIndexTag"/>.</returns>
    /// <exception cref="ParserOutOfBoundException">
    /// Lancée si la lecture dépasse la fin des données.
    /// </exception>
    public static SetTabIndexTag Read(SwfReader reader)
    {
        return new SetTabIndexTag(
            depth: reader.ReadUi16(),
            tabIndex: reader.ReadUi16()
        );
    }
}
