#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using JBSnorro.Diagnostics;
using JBSnorro.Text.Json;
using JBSnorro;
using JBSnorro.Extensions;
using BlaTeX.JSInterop.KaTeX;

namespace BlaTeX.JSInterop
{
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
            var polymorphicConverters = new[] {
                new ExactPolymorphicJsonConverter<HtmlDomNode>(
                    (typeof(DomSpan), typeof(_DomSpan)),
                    (typeof(HtmlDomNode), typeof(_HtmlDomNode))
                )
            };

            var introducedConverters = new HashSet<JsonConverter>();
            foreach (var polymorphicConverter in polymorphicConverters)
            {
                yield return polymorphicConverter;
                foreach (var introducedConverter in polymorphicConverter.IntroducedConverters)
                    introducedConverters.Add(introducedConverter);
            }
            foreach (var introducedConverter in introducedConverters)
                yield return introducedConverter;
        }
    }
}