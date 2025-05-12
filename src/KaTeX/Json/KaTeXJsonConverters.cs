using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections;
using BlaTeX.JSInterop.KaTeX.Internal;

namespace BlaTeX.JSInterop;

public static partial class KaTeXJsonConverters
{
    public static IReadOnlyList<JsonConverter> Converters { get; } = new ReadOnlyCollection<JsonConverter>(getJsonConverters().ToArray());
    public static void AddKaTeXJsonConverters(this JsonSerializerOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        foreach (var katexJsonConverter in Converters)
        {
            options.Converters.Add(katexJsonConverter);
        }
    }
    private static IEnumerable<JsonConverter> getJsonConverters()
    {
        var rootConverters = new JsonConverterCollection[]
        {
            new ExactPolymorphicJsonConverter<IHtmlDomNode>(
                (typeof(IDomSpan), typeof(DomSpan)),
                (typeof(IHtmlDomNode), typeof(HtmlDomNode))
                ),
            Attributes.JsonConverter.Instance,
            SourceLocation.JsonConverter.Instance,
            NodeTypeExtensions.JsonConverterInstance,
            AnyParseNode.Instances,
            JsonElementJsonConverter.Instance,
        };

        var result = new HashSet<JsonConverter>(rootConverters.SelectMany(_ => _)!);
        return result;
    }
}

public struct JsonConverterCollection : IEnumerable<JsonConverter?>
{
    private IReadOnlyList<JsonConverterCollection>? converterCollections;
    private IReadOnlyList<JsonConverter>? converters;

    public JsonConverterCollection(JsonConverter instance, IEnumerable<JsonConverter> introducedConverters)
    {
        this.converters = introducedConverters.Prepend(instance).ToList();
        this.converterCollections = null;
    }
    public JsonConverterCollection(JsonConverterCollection instance, IEnumerable<JsonConverterCollection> introducedConverters)
    {
        this.converterCollections = introducedConverters.Prepend(instance).ToList();
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
