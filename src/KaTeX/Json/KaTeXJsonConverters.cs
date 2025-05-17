using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections;
using BlaTeX.JSInterop.KaTeX.Internal;

namespace BlaTeX.JSInterop;

public static partial class KaTeXJsonConverters
{
    public static IReadOnlyList<JsonConverter> Converters { get; } = getJsonConverters();
    public static void AddKaTeXJsonConverters(this JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        foreach (var katexJsonConverter in Converters)
        {
            options.Converters.Add(katexJsonConverter);
        }
    }
    private static IReadOnlyList<JsonConverter> getJsonConverters()
    {
        JsonConverterCollection[] rootConverters = [
            new ExactPolymorphicJsonConverter<IHtmlDomNode>(
                (typeof(IDomSpan), typeof(KaTeX.Internal.Span<IHtmlDomNode>)),
                (typeof(IHtmlDomNode), typeof(HtmlDomNode)),
                (typeof(IAnchor), typeof(Anchor))
            ),
            Attributes.JsonConverter.Instance,
            SourceLocation.JsonConverter.Instance,
            NodeTypeExtensions.JsonConverterInstance,
            AnyParseNode.Instances,
            JsonElementJsonConverter.Instance,
        ];

        return rootConverters.SelectMany(_ => _).Unique().ToReadOnlyList()!;
    }
}

internal sealed class JsonConverterCollection : IEnumerable<JsonConverter?>
{
    private readonly IReadOnlyList<JsonConverterCollection>? converterCollections;
    private readonly IReadOnlyList<JsonConverter>? converters;

    public JsonConverterCollection(JsonConverter instance, IEnumerable<JsonConverter> introducedConverters)
    {
        this.converters = [instance, .. introducedConverters];
        this.converterCollections = null;
    }
    public JsonConverterCollection(JsonConverterCollection instance, IEnumerable<JsonConverterCollection> introducedConverters)
    {
        this.converterCollections = [instance, .. introducedConverters];
        this.converters = null;
    }

    public IEnumerator<JsonConverter?> GetEnumerator()
    {
        if (converters == null && converterCollections == null)
            throw new InvalidOperationException("Cannot enumerate default " + nameof(JsonConverterCollection));

        if (converters != null)
            foreach (var c in converters)
                yield return c;
        if (converterCollections != null)
            foreach (var collection in converterCollections)
                foreach (var c in collection)
                    yield return c;
    }


    public static implicit operator JsonConverterCollection(JsonConverter converter)
    {
        return new JsonConverterCollection(converter, Enumerable.Empty<JsonConverter>());
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
