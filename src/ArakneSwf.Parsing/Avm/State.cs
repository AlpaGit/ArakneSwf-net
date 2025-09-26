namespace ArakneSwf.Parsing.Avm;

/// <summary>
/// Store the current state of the AVM.
/// </summary>
public sealed class State
{
    /// <summary>
    /// Constants pool (index-based).
    /// </summary>
    public List<string> Constants { get; } = new();

    /// <summary>
    /// The execution stack.
    /// </summary>
    public List<object?> Stack { get; } = new();

    /// <summary>
    /// Current global variables.
    /// </summary>
    public Dictionary<string, object?> Variables { get; } =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Global functions.
    /// </summary>
    public Dictionary<string, Delegate> Functions { get; } =
        new(StringComparer.Ordinal);
}