using System.Text.Json.Serialization;
using ArakneSwf.Parsing.Extractor.Shapes.FillTypes;
using ArakneSwf.Parsing.Parser.Structure.Record;

namespace ArakneSwf.Parsing.Extractor;


[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never // keep output stable for hashing
)]
[JsonSerializable(typeof(LinearGradient))]
[JsonSerializable(typeof(RadialGradient))]
[JsonSerializable(typeof(Matrix))]
[JsonSerializable(typeof(Gradient))]
internal partial class SwfJsonContext : JsonSerializerContext
{
}