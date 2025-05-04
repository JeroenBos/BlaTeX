using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlaTeX.JSInterop.KaTeX;

public class _AnyParseNode : AnyParseNode
{
    public static JsonConverterCollection Instances { get; }
    private static JsonConverter<_AnyParseNode> UnspecificInstance { get; } = ExplicitJsonConverter<_AnyParseNode>.Create(typeof(_AnyParseNode));
    static _AnyParseNode()
    {
        // HACK: replace GetDeserializationType with GetASTType when all node subtypes are supported. In that case make _AnyParseNode abstract?
        Type GetDeserializationType(NodeType type)
        {
            var result = NodeTypeExtensions.GetASTType(type);

            // this will trigger a default node to be created instead of a specialized node
            return result ?? typeof(_AnyParseNode);
        }

        var instance = new ExplicitPolymorphicJsonConverter<AnyParseNode, NodeType>(
            "type",
            GetDeserializationType,
            getSerializerOrTypeKey: _ => (typeof(_AnyParseNode), null)
        );
        var subtypeConverters = new JsonConverter[] {
            _BlaTeXNode.JsonConverterInstance,
            UnspecificInstance,
            ModeExtensions.Instance,
        };
        Instances = new JsonConverterCollection(instance, subtypeConverters);
    }

    class _AnyParseNodeJsonConverter : JsonConverter<_AnyParseNode>
    {
        public override _AnyParseNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (_AnyParseNode)DefaultObjectJsonConverter.Read(ref reader, typeof(_AnyParseNode), options)!;
#pragma warning disable CS0162
            var properties = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader, options)
                                            .Map<string, JsonElement, object>((key, jsonElement) => JsonElementExtensions.Deserialize<object>(jsonElement, options));

            var type = (NodeType)properties["type"]!;
            var mode = (Mode)properties["mode"]!;
            var loc = (SourceLocation?)properties["loc"];
            var result = new _AnyParseNode(type, mode, loc);
            properties.Remove("type");
            properties.Remove("mode");
            properties.Remove("loc");
            result.UnspecificProperties = properties;
            return result;
        }

        public override void Write(Utf8JsonWriter writer, _AnyParseNode value, JsonSerializerOptions options)
        {
            var d = value.UnspecificProperties ?? new Dictionary<string, object?>();

            d["type"] = value.type;
            d["mode"] = value.mode;
            d["loc"] = value.loc;

            JsonSerializer.Serialize(writer, d, options);
        }
    }
    /// <summary> This replaces the need for a dedicated type for each NodeType. </summary>
    [JsonExtensionData]
    public IDictionary<string, object?>? UnspecificProperties { get; set; }

    public NodeType type { get; set; }
    public Mode mode { get; set; }
    // name determined by KaTeX (and used in (de)serialization)
    public SourceLocation? loc { get; set; }

    NodeType AnyParseNode.Type => type;
    Mode AnyParseNode.Mode => mode;
    SourceLocation? AnyParseNode.SourceLocation => loc;

    /// <summary> Ctor for JsonSerializer. </summary>
    public _AnyParseNode() { }
    public _AnyParseNode(NodeType type,
                            Mode mode,
                            SourceLocation? sourceLocation)
    {
        this.type = type;
        this.mode = mode;
        this.loc = sourceLocation;
    }


    public override bool Equals(object? obj)
    {
        return this.Equals(obj as AnyParseNode);
    }
    protected bool Equals([NotNullWhen(true)] AnyParseNode? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(other, this)) // optimization
            return true;

        if (this.type != other.Type)
            return false;
        if (this.mode != other.Mode)
            return false;
        if (this.loc?.Equals(other.SourceLocation) ?? other.SourceLocation is { })
            return false;
        return true;
    }
    public override int GetHashCode() => throw new NotImplementedException();

    public virtual AnyParseNode With(Option<NodeType> type = default,
                                        Option<Mode> mode = default,
                                        Option<SourceLocation?> sourceLocation = default)
    {
        return new _AnyParseNode(
            type.ValueOrDefault(this.type),
            mode.ValueOrDefault(this.mode),
            sourceLocation.ValueOrDefault(this.loc)
        );
    }
}

public class _BlaTeXNode : _AnyParseNode, BlaTeXNode
{
    public static JsonConverter<BlaTeXNode> JsonConverterInstance => ExactJsonConverter<BlaTeXNode, _BlaTeXNode>.Instance;
    public required _AnyParseNode[] args { get; init; }

    _AnyParseNode[] BlaTeXNode.Args => args;

    /// <summary> Ctor for JsonSerializer. </summary>
    public _BlaTeXNode() { }
    [SetsRequiredMembers]
    public _BlaTeXNode(NodeType type,
                       Mode mode,
                       SourceLocation? sourceLocation,
                       _AnyParseNode[] args)
        : base(type, mode, sourceLocation)
    {
        this.args = args;
    }


    public override bool Equals(object? obj)
    {
        return this.Equals(obj as BlaTeXNode);
    }
    protected bool Equals([NotNullWhen(true)] BlaTeXNode? other)
    {
        if (!base.Equals(other))
            return false;

        if (this.args?.SequenceEqual(other.Args) ?? other.Args is not null)
            return false;
        return true;
    }
    public override int GetHashCode() => throw new NotImplementedException();

    public sealed override AnyParseNode With(Option<NodeType> type = default,
                                                Option<Mode> mode = default,
                                                Option<SourceLocation?> sourceLocation = default)
    {
        return this.With(type, mode, sourceLocation, default);
    }
    public virtual BlaTeXNode With(Option<NodeType> type = default,
                                    Option<Mode> mode = default,
                                    Option<SourceLocation?> sourceLocation = default,
                                    Option<_AnyParseNode[]> args = default)
    {
        return new _BlaTeXNode(type.ValueOrDefault(this.type),
                                mode.ValueOrDefault(this.mode),
                                sourceLocation.ValueOrDefault(this.loc),
                                args.ValueOrDefault(this.args) ?? []);
    }
}
