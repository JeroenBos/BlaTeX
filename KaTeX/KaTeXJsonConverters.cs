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
using BlaTeX.JSInterop.KaTeX.Syntax;

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
            var rootConverters = new JsonConverter[]
            {
                new ExactPolymorphicJsonConverter<HtmlDomNode>(
                    (typeof(DomSpan), typeof(_DomSpan)),
                    (typeof(HtmlDomNode), typeof(_HtmlDomNode))
                    ),
                _Attributes.JsonConverter.Instance,
                _SourceLocation.JsonConverter.Instance,
                NodeTypeExtensions.JsonConverterInstance,
                _AnyParseNode.Instance,
            };

            var result = new HashSet<JsonConverter>(
                rootConverters.TransitiveSelect(IJsonConcerterIntroducerExtensions.TryGetIntroducedConverters)
            );
            return result;
        }
    }
    /// <summary> Deserializes objects polymorphically by using a property that serves as key to identify the actual
    /// type of the deserialized object, and delegates to the corresponding deserializer. </summary>
    public class ExplicitPolymorphicJsonConverter<T, TKey> : JsonConverter<T>
    {
        private readonly string KeyPropertyName;
        private readonly Func<TKey, Type> GetTypeToDeserialize;
        private readonly IEqualityComparer<string> KeyPropertyNameEqualityComparer;
        public ExplicitPolymorphicJsonConverter(string keyPropertyName,
                                                Func<TKey, Type> getTypeToDeserialize,
                                                IEqualityComparer<string>? keyPropertyNameEqualityComparer = null)
        {
            Contract.Requires(keyPropertyName != null);
            Contract.Requires(getTypeToDeserialize != null);

            KeyPropertyName = keyPropertyName;
            GetTypeToDeserialize = getTypeToDeserialize;
            KeyPropertyNameEqualityComparer = keyPropertyNameEqualityComparer ?? EqualityComparer<string>.Default;
        }

        public override T Read(ref Utf8JsonReader _reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Utf8JsonReader reader = _reader;
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected object to get property of for explicit type mapping");

            reader.Read();
            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                string keyName = reader.GetString();
                if (KeyPropertyNameEqualityComparer.Equals(keyName, this.KeyPropertyName))
                {
                    reader.Read();
                    TKey keyValue = JsonSerializer.Deserialize<TKey>(ref reader, options);
                    Type type = this.GetTypeToDeserialize(keyValue);
                    if (type == typeof(T))
                    {
                        throw new ContractException($"{nameof(GetTypeToDeserialize)} may not return 'T' (={typeof(T).FullName})");
                    }

                    var result = JsonSerializer.Deserialize(ref _reader /*continue with original reader*/, type, options);
                    return (T)(object)result;
                }
                else
                {
                    SkipProperty(ref reader, options);
                }
            }
            throw new JsonException($"Key '{this.KeyPropertyName}' not found. ");
        }
        static void SkipProperty(ref Utf8JsonReader r, JsonSerializerOptions options)
        {
            r.Read();
            r.GetTokenAsJson(); // just propagate the reader: not checking anything
            r.Read(); // read the endobject/endarray of the previous property
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotSupportedException("The writer of the derived type should have been resolved instead of this writer");
            // alternatively we could extract the key from 'value' (using reflection?) and instead do
            // JsonSerializer.Serialize(writer, value, GetTypeToDeserialize(keyValue), options);
        }
    }

    /// <summary> Serializes values of the specified enum type by name and deserializes them from string (case-insensitive). </summary>
    public class JsonStringEnumConverter<T> : JsonConverter<T> where T : Enum
    {
        private static readonly JsonSerializerOptions options;
        public static JsonStringEnumConverter<T> Instance { get; }
        static JsonStringEnumConverter()
        {
            options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            Instance = new JsonStringEnumConverter<T>();
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            string name = Enum.GetName(typeof(T), value);
            JsonSerializer.Serialize<string>(name, options);
        }
    }
    /// <summary> Indicates that the implementing json converter introduces/requires other json converter. </summary>
    public interface IJsonConverterIntroducer
    {
        /// <summary> Get all converts introduced/required by the current converter. </summary>
        IEnumerable<JsonConverter> IntroducedConverters { get; }
    }
    public class JsonConverterIntroducerWrapper : IJsonConverterIntroducer
    {
        public JsonConverter Instance { get; }
        public IEnumerable<JsonConverter> IntroducedConverters { get; }

        public JsonConverterIntroducerWrapper(JsonConverter instance, IEnumerable<JsonConverter> introducedConverters)
        {
            Contract.Requires(instance != null);
            Contract.Requires(introducedConverters != null);

            this.Instance = instance;
            this.IntroducedConverters = introducedConverters;
        }
    }
    public static class IJsonConcerterIntroducerExtensions
    {
        public static IEnumerable<JsonConverter> TryGetIntroducedConverters(this JsonConverter converter)
        {
            return converter switch
            {
                IJsonConverterIntroducer introducer => introducer.IntroducedConverters,
                _ => Enumerable.Empty<JsonConverter>()
            };
        }
    }
}