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
using System.Collections;

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
			var rootConverters = new JsonConverterCollection[]
			{
				new ExactPolymorphicJsonConverter<HtmlDomNode>(
					(typeof(DomSpan), typeof(_DomSpan)),
					(typeof(HtmlDomNode), typeof(_HtmlDomNode))
					),
				_Attributes.JsonConverter.Instance,
				_SourceLocation.JsonConverter.Instance,
				NodeTypeExtensions.JsonConverterInstance,
				_AnyParseNode.Instances,
				JsonElementJsonConverter.Instance,
			};

			var result = new HashSet<JsonConverter>(rootConverters.SelectMany(_ => _)!);
			return result;
		}
	}
	/// <summary> Deserializes objects polymorphically by using a property that serves as key to identify the actual
	/// type of the deserialized object, and delegates to the corresponding deserializer. </summary>
	public class ExplicitPolymorphicJsonConverter<T, TKey> : JsonConverter<T>
	{
		private readonly string keyPropertyName;
		private readonly Func<TKey, Type> getTypeToDeserialize;
		private readonly IEqualityComparer<string> keyPropertyNameEqualityComparer;
		private readonly Func<T, (Type?, JsonConverter<T>?)>? getSerializerOrTypeKey;
		/// <summary>
		/// <param name="getSerializerOrTypeKey"> This type does not support writing, but you can delegate to another converter with this function. </param>
		/// </summary>
		public ExplicitPolymorphicJsonConverter(string keyPropertyName,
												Func<TKey, Type> getTypeKeyToDeserialize,
												IEqualityComparer<string>? keyPropertyNameEqualityComparer = null,
												Func<T, (Type?, JsonConverter<T>?)>? getSerializerOrTypeKey = null)
		{
			Contract.Requires(keyPropertyName != null);
			Contract.Requires(getTypeKeyToDeserialize != null);

			this.keyPropertyName = keyPropertyName;
			this.getTypeToDeserialize = getTypeKeyToDeserialize;
			this.keyPropertyNameEqualityComparer = keyPropertyNameEqualityComparer ?? EqualityComparer<string>.Default;
			this.getSerializerOrTypeKey = getSerializerOrTypeKey;
		}

		public override T Read(ref Utf8JsonReader _reader, Type typeToConvert, JsonSerializerOptions options)
		{
			using var _ = this.DetectStackoverflow(_reader, typeToConvert);

			Utf8JsonReader reader = _reader;
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object to get property of for explicit type mapping");

			reader.Read();
			while (reader.TokenType == JsonTokenType.PropertyName)
			{
				string keyName = reader.GetString();
				if (keyPropertyNameEqualityComparer.Equals(keyName, this.keyPropertyName))
				{
					reader.Read();
					TKey keyValue = JsonSerializer.Deserialize<TKey>(ref reader, options);
					Type type = this.getTypeToDeserialize(keyValue);
					if (type == typeof(T))
					{
						throw new ContractException($"{nameof(getTypeToDeserialize)} may not return 'T' (={typeof(T).FullName})");
					}

					var result = JsonSerializer.Deserialize(ref _reader /*continue with original reader*/, type, options);
					return (T)(object)result;
				}
				else
				{
					SkipProperty(ref reader, options);
				}
			}
			throw new JsonException($"Key '{this.keyPropertyName}' not found. ");
		}
		static void SkipProperty(ref Utf8JsonReader r, JsonSerializerOptions options)
		{
			r.Read();
			r.GetTokenAsJson(); // just propagate the reader: not checking anything
			r.Read(); // read the endobject/endarray of the previous property
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			if (getSerializerOrTypeKey == null)
				throw new NotSupportedException($"Serialization not supported. {nameof(getSerializerOrTypeKey)} is null and this method has not been overridden");

			var (key, converter) = getSerializerOrTypeKey(value);
			if ((key == null) == (converter == null))
				throw new ContractException($"{nameof(getSerializerOrTypeKey)} must return either one of the two return type elements");
			if (key == typeof(T))
				throw new ContractException($"{nameof(getSerializerOrTypeKey)} must return not return this converter's key");
			if (converter == this)
				throw new ContractException($"{nameof(getSerializerOrTypeKey)} must return not return this converter");

			if (key != null)
			{
				using var _ = this.DetectStackoverflow(writer, typeof(T));
				JsonSerializer.Serialize(writer, value, key, options);
			}
			else
			{
				converter!.Write(writer, value, options);
			}
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

}
