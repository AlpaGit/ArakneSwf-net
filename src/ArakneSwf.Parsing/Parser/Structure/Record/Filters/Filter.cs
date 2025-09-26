using ArakneSwf.Parsing.Error;

namespace ArakneSwf.Parsing.Parser.Structure.Record.Filters;

/// <summary>
/// Base type for graphic filters.
/// </summary>
public abstract class Filter
{
    /// <summary>
    /// Reads a collection of filters from the SWF reader.
    /// The collection size is provided by the first byte.
    /// </summary>
    /// <remarks>
    /// Si un type de filtre inconnu est rencontré :
    /// - avec <see cref="Errors.InvalidData"/> actif, une <see cref="ParserInvalidDataException"/> est levée ;
    /// - sinon, le filtre est ignoré (seul l’octet d’identifiant aura été lu).
    /// </remarks>
    public static List<Filter> ReadCollection(SwfReader reader)
    {
        var filters = new List<Filter>();
        int count = reader.ReadUi8();
        var end = reader.End;

        for (var f = 0; f < count && reader.Offset < end; f++)
        {
            var filterId = reader.ReadUi8();

            Filter? filter = filterId switch
            {
                DropShadowFilter.FILTER_ID    => DropShadowFilter.Read(reader),
                BlurFilter.FILTER_ID          => BlurFilter.Read(reader),
                GlowFilter.FILTER_ID          => GlowFilter.Read(reader),
                BevelFilter.FILTER_ID         => BevelFilter.Read(reader),
                GradientGlowFilter.FILTER_ID  => GradientGlowFilter.Read(reader),
                ConvolutionFilter.FILTER_ID   => ConvolutionFilter.Read(reader),
                ColorMatrixFilter.FILTER_ID   => ColorMatrixFilter.Read(reader),
                GradientBevelFilter.FILTER_ID => GradientBevelFilter.Read(reader),

                _ => (reader.Errors & Errors.InvalidData) != 0
                    ? throw new ParserInvalidDataException($"Unknown filter type {filterId}", reader.Offset)
                    : null
            };

            if (filter is not null)
                filters.Add(filter);
        }

        return filters;
    }
}