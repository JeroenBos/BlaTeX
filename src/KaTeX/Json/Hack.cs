using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlaTeX.JSInterop.KaTeX;

internal class IJSSerializableState
{
    // HACK HACK HACK id=0 because I can't get the System.Text.Json.JsonSerializer/Converter/Options to do what I want.... 
    // which is to add a the property 'SERIALIZATION_TYPE_ID' to the following list of types:
    internal static IReadOnlyList<Type> convertibleTypes = [
        // the only classes in KaTeX that implement VirtualNode (in js) are SvgNode, PathNode, LineNode, MathDomNode, 
        // and the interface HtmlDomNode, which is implemented only by Span, Anchor, SymbolNode and DocumentFragment 
        // Complicatedly, DocumentFragment also derives from MathDomNode. But we'll see that when we get to it

        // uncomment these when they're available:
        // typeof(DocumentFragment),
        typeof(IMathDomNode),
        // typeof(SvgNode),
        // typeof(PathNode),
        // typeof(LineNode),
        typeof(ISpan<>),
        // typeof(SymbolNode),
        // typeof(Anchor),
    ];
}
internal interface IJSSerializable
{
    public static string SERIALIZATION_TYPE_ID_Impl(IJSSerializable @this)
    {
        if (@this == null)
            throw new ArgumentNullException(nameof(@this));


        int index = IJSSerializableState.convertibleTypes.IndexOf(t => t.IsAssignableFrom(@this.GetType()));
        if (index == -1)
            throw new ArgumentException($"The type '{@this.GetType().FullName}' is not JS serializable");
        return "v1_" + index.ToString();
    }
    string SERIALIZATION_TYPE_ID { get; }
}

/// <summary>
/// Deserializes objects polymorphically by using a property that serves as key to identify the actual
/// type of the deserialized object, and delegates to the corresponding deserializer.
/// </summary>
public class ExplicitPolymorphicJsonConverter<T, TKey> : JsonConverter<T>
{
    private readonly string keyPropertyName;
    private readonly Func<TKey, Type> getTypeToDeserialize;
    private readonly IEqualityComparer<string> keyPropertyNameEqualityComparer;
    private readonly Func<T, Either<Type, JsonConverter<T>>>? getSerializerOrTypeKey;
    /// <param name="getSerializerOrTypeKey"> This type but this allows you to delegate to another converter resolve it via another key type. </param>
    public ExplicitPolymorphicJsonConverter(string keyPropertyName,
                                            Func<TKey, Type> getTypeKeyToDeserialize,
                                            Func<T, Either<Type, JsonConverter<T>>>? getSerializerOrTypeKey = null,
                                            IEqualityComparer<string>? keyPropertyNameEqualityComparer = null)
    {
        Contract.Requires(keyPropertyName != null);
        Contract.Requires(getTypeKeyToDeserialize != null);

        this.keyPropertyName = keyPropertyName;
        this.getTypeToDeserialize = getTypeKeyToDeserialize;
        this.getSerializerOrTypeKey = getSerializerOrTypeKey;
        this.keyPropertyNameEqualityComparer = keyPropertyNameEqualityComparer ?? EqualityComparer<string>.Default;
    }

    public override T? Read(ref Utf8JsonReader _reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var _ = this.DetectStackoverflow(_reader, typeToConvert);

        Utf8JsonReader reader = _reader;
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected object to get property of for explicit type mapping");

        reader.Read();
        while (reader.TokenType == JsonTokenType.PropertyName)
        {
            string? keyName = reader.GetString();
            if (keyPropertyNameEqualityComparer.Equals(keyName, this.keyPropertyName))
            {
                reader.Read();
                TKey? keyValue = JsonSerializer.Deserialize<TKey>(ref reader, options);
                Contract.Assert<JsonException>(keyValue is not null, "key must not be null");

                Type type = this.getTypeToDeserialize(keyValue);
                if (type == typeof(T))
                {
                    throw new ContractException($"{nameof(getTypeToDeserialize)} may not return 'T' (={typeof(T).FullName})");
                }

                object? result = JsonSerializer.Deserialize(ref _reader /*continue with original reader*/, type, options);
                return (T?)result;
            }
            else
            {
                SkipProperty(ref reader);
            }
        }
        throw new JsonException($"Key '{this.keyPropertyName}' not found");
    }
    static void SkipProperty(ref Utf8JsonReader r)
    {
        r.Read();
        r.GetTokenAsJson(); // just propagate the reader: not checking anything
        r.Read(); // read the endobject/endarray of the previous property
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (getSerializerOrTypeKey == null)
            throw new NotSupportedException($"Serialization not supported as no '{nameof(getSerializerOrTypeKey)}' has been provided in the constructor");

        var either = getSerializerOrTypeKey(value);
        if (either.Get(out Type key, out JsonConverter<T> converter))
        {
            Contract.Requires(key != typeof(T), $"{nameof(getSerializerOrTypeKey)} must return not return this converter's key");
            using var _ = this.DetectStackoverflow(writer, typeof(T));
            JsonSerializer.Serialize(writer, value, key, options);
        }
        else
        {
            Contract.Requires(!ReferenceEquals(converter, this), $"{nameof(getSerializerOrTypeKey)} must return not return this converter");
            converter.Write(writer, value, options);
        }
    }
}
