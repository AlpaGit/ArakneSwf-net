namespace ArakneSwf.Parsing.Parser.Structure.Action;

/// <summary>
/// Données pour l'action GetURL (URL + cible).
/// </summary>
public sealed class GetUrlData
{
    public string Url { get; }
    public string Target { get; }

    public GetUrlData(string url, string target)
    {
        Url = url;
        Target = target;
    }

    public static GetUrlData Read(SwfReader reader)
    {
        var url = reader.ReadNullTerminatedString();
        var target = reader.ReadNullTerminatedString();
        return new GetUrlData(url, target);
    }
}
