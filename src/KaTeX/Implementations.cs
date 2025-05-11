using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlaTeX.JSInterop.KaTeX;

internal class AnyParseNode : IAnyParseNode
{
    public static JsonConverterCollection Instances { get; }
    private static JsonConverter<AnyParseNode> UnspecificInstance { get; } = ExplicitJsonConverter<AnyParseNode>.Create(typeof(AnyParseNode));
    static AnyParseNode()
    {
        // HACK: replace GetDeserializationType with GetASTType when all node subtypes are supported. In that case make _AnyParseNode abstract?
        Type GetDeserializationType(NodeType type)
        {
            var result = NodeTypeExtensions.GetASTType(type);

            // this will trigger a default node to be created instead of a specialized node
            return result ?? typeof(AnyParseNode);
        }

        var instance = new ExplicitPolymorphicJsonConverter<IAnyParseNode, NodeType>(
            "type",
            GetDeserializationType,
            getSerializerOrTypeKey: _ => (typeof(AnyParseNode), null)
        );
        var subtypeConverters = new JsonConverter[] {
            BlaTeXNode.JsonConverterInstance,
            UnspecificInstance,
            ModeExtensions.Instance,
        };
        Instances = new JsonConverterCollection(instance, subtypeConverters);
    }

    class _AnyParseNodeJsonConverter : JsonConverter<AnyParseNode>
    {
        public override AnyParseNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return (AnyParseNode)DefaultObjectJsonConverter.Read(ref reader, typeof(AnyParseNode), options)!;
#pragma warning disable CS0162
            var properties = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(ref reader, options)
                                            .Map<string, JsonElement, object>((key, jsonElement) => JsonElementExtensions.Deserialize<object>(jsonElement, options));

            var type = (NodeType)properties["type"]!;
            var mode = (Mode)properties["mode"]!;
            var loc = (ISourceLocation?)properties["loc"];
            var result = new AnyParseNode(type, mode, loc);
            properties.Remove("type");
            properties.Remove("mode");
            properties.Remove("loc");
            result.UnspecificProperties = properties;
            return result;
        }

        public override void Write(Utf8JsonWriter writer, AnyParseNode value, JsonSerializerOptions options)
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
    public ISourceLocation? loc { get; set; }

    NodeType IAnyParseNode.Type => type;
    Mode IAnyParseNode.Mode => mode;
    ISourceLocation? IAnyParseNode.SourceLocation => loc;

    /// <summary> Ctor for JsonSerializer. </summary>
    public AnyParseNode() { }
    public AnyParseNode(NodeType type,
                            Mode mode,
                            ISourceLocation? sourceLocation)
    {
        this.type = type;
        this.mode = mode;
        this.loc = sourceLocation;
    }


    public override bool Equals(object? obj)
    {
        return this.Equals(obj as IAnyParseNode);
    }
    protected bool Equals([NotNullWhen(true)] IAnyParseNode? other)
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

    public virtual IAnyParseNode With(Option<NodeType> type = default,
                                        Option<Mode> mode = default,
                                        Option<ISourceLocation?> sourceLocation = default)
    {
        return new AnyParseNode(
            type.ValueOrDefault(this.type),
            mode.ValueOrDefault(this.mode),
            sourceLocation.ValueOrDefault(this.loc)
        );
    }
}

internal class BlaTeXNode : AnyParseNode, IBlaTeXNode
{
    public static JsonConverter<IBlaTeXNode> JsonConverterInstance => ExactJsonConverter<IBlaTeXNode, BlaTeXNode>.Instance;
    public required AnyParseNode[] args { get; init; }

    IAnyParseNode[] IBlaTeXNode.Args => args;

    /// <summary> Ctor for JsonSerializer. </summary>
    public BlaTeXNode() { }
    [SetsRequiredMembers]
    public BlaTeXNode(NodeType type,
                       Mode mode,
                       ISourceLocation? sourceLocation,
                       AnyParseNode[] args)
        : base(type, mode, sourceLocation)
    {
        this.args = args;
    }


    public override bool Equals(object? obj)
    {
        return this.Equals(obj as IBlaTeXNode);
    }
    protected bool Equals([NotNullWhen(true)] IBlaTeXNode? other)
    {
        if (!base.Equals(other))
            return false;

        if (this.args?.SequenceEqual(other.Args) ?? other.Args is not null)
            return false;
        return true;
    }
    public override int GetHashCode() => throw new NotImplementedException();

    public sealed override IAnyParseNode With(Option<NodeType> type = default,
                                                Option<Mode> mode = default,
                                                Option<ISourceLocation?> sourceLocation = default)
    {
        return this.With(type, mode, sourceLocation, default);
    }
    public virtual IBlaTeXNode With(Option<NodeType> type = default,
                                    Option<Mode> mode = default,
                                    Option<ISourceLocation?> sourceLocation = default,
                                    Option<AnyParseNode[]> args = default)
    {
        return new BlaTeXNode(type.ValueOrDefault(this.type),
                                mode.ValueOrDefault(this.mode),
                                sourceLocation.ValueOrDefault(this.loc),
                                args.ValueOrDefault(this.args) ?? []);
    }
}
