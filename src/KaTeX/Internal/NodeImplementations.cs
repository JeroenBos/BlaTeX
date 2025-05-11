using JBSnorro.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BlaTeX.JSInterop.KaTeX.Internal;

internal class HtmlDomNode : IHtmlDomNode, IJSSerializable
{
    public string SERIALIZATION_TYPE_ID => IJSSerializable.SERIALIZATION_TYPE_ID_Impl(this);
    public IReadOnlyList<string>? classes { get; set; }
    public float height { get; set; }
    public float depth { get; set; }
    public float maxFontSize { get; set; }
    public CssStyle? style { get; set; }
    [DebuggerHidden]
    IReadOnlyList<string>? IHtmlDomNode.Classes => classes;
    [DebuggerHidden]
    float IHtmlDomNode.Height => height;
    [DebuggerHidden]
    float IHtmlDomNode.Depth => depth;
    [DebuggerHidden]
    float IHtmlDomNode.MaxFontSize => maxFontSize;
    [DebuggerHidden]
    ICssStyle? IHtmlDomNode.Style => style;


    /// <summary> Ctor for JsonSerializer. </summary>
    public HtmlDomNode() { }
    public HtmlDomNode(
        IReadOnlyList<string>? classes,
        float height,
        float depth,
        float maxFontSize,
        CssStyle? style)
    {
        this.classes = classes;
        this.height = height;
        this.depth = depth;
        this.maxFontSize = maxFontSize;
        this.style = style;
    }


    public override bool Equals(object? obj)
    {
        return this.Equals(obj as IHtmlDomNode);
    }

    protected bool Equals([NotNullWhen(true)] IHtmlDomNode? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(other, this)) // optimization
            return true;

        if (this.depth != other.Depth)
            return false;
        if (this.height != other.Height)
            return false;
        if (this.maxFontSize != other.MaxFontSize)
            return false;
        if (this.style?.Equals(other.Style) ?? other.Style is { })
            return false;
        if (this.classes?.SequenceEqual(other.Classes!) ?? other.Classes is { })
            return false;
        return true;
    }
    public override int GetHashCode() => throw new NotImplementedException();

    public virtual IHtmlDomNode With(Option<IReadOnlyList<string>> classes = default,
                                    Option<float> height = default,
                                    Option<float> depth = default,
                                    Option<float> maxFontSize = default,
                                    Option<ICssStyle> style = default)
    {
        return new HtmlDomNode(classes.ValueOrDefault(this.classes),
                                height.ValueOrDefault(this.height),
                                depth.ValueOrDefault(this.depth),
                                maxFontSize.ValueOrDefault(this.maxFontSize),
                                (CssStyle?)style.ValueOrDefault(this.style));
    }
}

/// <summary> A concrete type of Span<T>. </summary>
internal class DomSpan : Span<IHtmlDomNode>, IDomSpan
{
    /// <summary> Ctor for JsonSerializer. </summary>
    public DomSpan()
    {

    }
    public DomSpan(
        IHtmlDomNode[]? children,
        IAttributes? attributes,
        float? width,
        IReadOnlyList<string>? classes,
        float height,
        float depth,
        float maxFontSize,
        CssStyle? style)
        : base(children, attributes, width, classes, height, depth, maxFontSize, style)
    {
    }
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as IDomSpan);
    }
    protected bool Equals([NotNullWhen(true)] IDomSpan? other)
    {
        return base.Equals(other);
    }
    public override int GetHashCode() => throw new NotImplementedException();

    public sealed override ISpan<IHtmlDomNode> With(Option<IHtmlDomNode[]> children = default,
                                                    Option<IAttributes> attributes = default,
                                                    Option<float?> width = default,
                                                    Option<IReadOnlyList<string>> classes = default,
                                                    Option<float> height = default,
                                                    Option<float> depth = default,
                                                    Option<float> maxFontSize = default,
                                                    Option<ICssStyle> style = default)
    {
        return new DomSpan(children.ValueOrDefault(this.children),
                            attributes.ValueOrDefault(this.attributes),
                            width.ValueOrDefault(this.width),
                            classes.ValueOrDefault(this.classes),
                            height.ValueOrDefault(this.height),
                            depth.ValueOrDefault(this.depth),
                            maxFontSize.ValueOrDefault(this.maxFontSize),
                            (CssStyle?)style.ValueOrDefault(this.style));
    }
}

internal class Span<TChildNode> : HtmlDomNode, ISpan<TChildNode> where TChildNode : IVirtualNode
{
    public TChildNode[] children { get; set; }
    public IAttributes attributes { get; set; }
    public float? width { get; set; }
    [DebuggerHidden]
    IReadOnlyList<TChildNode> ISpan<TChildNode>.Children => children;
    [DebuggerHidden]
    IAttributes ISpan<TChildNode>.Attributes => attributes;
    [DebuggerHidden]
    float? ISpan<TChildNode>.Width => width;

    /// <summary> Ctor for JsonSerializer. </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Span() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Span(TChildNode[]? children,
                 IAttributes? attributes,
                 float? width,
                 IReadOnlyList<string>? classes,
                 float height,
                 float depth,
                 float maxFontSize,
                 CssStyle? style)
        : base(classes, height, depth, maxFontSize, style)
    {
        this.children = children ?? [];
        this.attributes = attributes ?? IAttributes.Empty;
        this.width = width;
    }
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as ISpan<TChildNode>);
    }
    protected bool Equals([NotNullWhen(true)] ISpan<TChildNode>? other)
    {
        if (!base.Equals(other))
            return false;

        if (this.width != other.Width)
            return false;
        if (!this.children.SequenceEqual(other.Children))
            return false;
        if (this.attributes.Equals(other.Attributes))
            return false;
        return true;
    }
    public override int GetHashCode() => throw new NotImplementedException();
    public sealed override IHtmlDomNode With(Option<IReadOnlyList<string>> classes = default,
                                            Option<float> height = default,
                                            Option<float> depth = default,
                                            Option<float> maxFontSize = default,
                                            Option<ICssStyle> style = default)
    {
        return With(default, default, default, classes, height, depth, maxFontSize, style);
    }
    public virtual ISpan<TChildNode> With(Option<TChildNode[]> children = default,
                                         Option<IAttributes> attributes = default,
                                         Option<float?> width = default,
                                         Option<IReadOnlyList<string>> classes = default,
                                         Option<float> height = default,
                                         Option<float> depth = default,
                                         Option<float> maxFontSize = default,
                                         Option<ICssStyle> style = default)
    {
        return new Span<TChildNode>(children.ValueOrDefault(this.children),
                                     attributes.ValueOrDefault(this.attributes),
                                     width.ValueOrDefault(this.width),
                                     classes.ValueOrDefault(this.classes),
                                     height.ValueOrDefault(this.height),
                                     depth.ValueOrDefault(this.depth),
                                     maxFontSize.ValueOrDefault(this.maxFontSize),
                                     (CssStyle?)style.ValueOrDefault(this.style));
    }
}

internal class SettingsOptions : ISettingsOptions
{
    public bool? displayMode { get; set; }
    public bool? throwOnError { get; set; }
    public string? errorColor { get; set; }
    public IMacroMap? macros { get; set; }
    public bool? colorIsTextColor { get; set; }
    public IStrict? strict { get; set; }
    public float? maxSize { get; set; }
    public float? maxExpand { get; set; }
    public string[]? allowedProtocols { get; set; }

    bool? ISettingsOptions.DisplayMode => displayMode;
    bool? ISettingsOptions.ThrowOnError => throwOnError;
    string? ISettingsOptions.ErrorColor => errorColor;
    IMacroMap? ISettingsOptions.Macros => macros;
    bool? ISettingsOptions.ColorIsTextColor => colorIsTextColor;
    IStrict? ISettingsOptions.Strict => strict;
    float? ISettingsOptions.MaxSize => maxSize;
    float? ISettingsOptions.MaxExpand => maxExpand;
    IReadOnlyList<string>? ISettingsOptions.AllowedProtocols => allowedProtocols;

    /// <summary> Ctor for JsonSerializer. </summary>
    public SettingsOptions() { }
    public SettingsOptions(bool? displayMode,
                            bool? throwOnError,
                            string? errorColor,
                            IMacroMap? macros,
                            bool? colorIsTextColor,
                            IStrict? strict,
                            float? maxSize,
                            float? maxExpand,
                            string[]? allowedProtocols)
    {
        this.displayMode = displayMode;
        this.throwOnError = throwOnError;
        this.errorColor = errorColor;
        this.macros = macros;
        this.colorIsTextColor = colorIsTextColor;
        this.strict = strict;
        this.maxSize = maxSize;
        this.maxExpand = maxExpand;
        this.allowedProtocols = allowedProtocols;
    }


    public override bool Equals(object? obj)
    {
        return this.Equals(obj as ISettingsOptions);
    }
    protected bool Equals([NotNullWhen(true)] ISettingsOptions? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(other, this)) // optimization
            return true;

        if (this.displayMode != other.DisplayMode)
            return false;
        if (this.throwOnError != other.ThrowOnError)
            return false;
        if (this.errorColor != other.ErrorColor)
            return false;
        if (this.macros?.Equals(other.Macros) ?? other.Macros is { })
            return false;
        if (this.colorIsTextColor != other.ColorIsTextColor)
            return false;
        if (this.strict?.Equals(other.Strict) ?? other.Strict is { })
            return false;
        if (this.maxSize != other.MaxSize)
            return false;
        if (this.maxExpand != other.MaxExpand)
            return false;
        if (this.allowedProtocols is null)
            return other.AllowedProtocols is null;
        else
            return other.AllowedProtocols is not null && this.allowedProtocols.SequenceEqual(other.AllowedProtocols);
    }
    public override int GetHashCode() => throw new NotImplementedException();

    public virtual ISettingsOptions With(Option<bool?> displayMode = default,
                                        Option<bool?> throwOnError = default,
                                        Option<string?> errorColor = default,
                                        Option<IMacroMap?> macros = default,
                                        Option<bool?> colorIsTextColor = default,
                                        Option<IStrict?> strict = default,
                                        Option<float?> maxSize = default,
                                        Option<float?> maxExpand = default,
                                        Option<string[]?> allowedProtocols = default)
    {
        return new SettingsOptions(
            displayMode.ValueOrDefault(this.displayMode),
            throwOnError.ValueOrDefault(this.throwOnError),
            errorColor.ValueOrDefault(this.errorColor),
            macros.ValueOrDefault(this.macros),
            colorIsTextColor.ValueOrDefault(this.colorIsTextColor),
            strict.ValueOrDefault(this.strict),
            maxSize.ValueOrDefault(this.maxSize),
            maxExpand.ValueOrDefault(this.maxExpand),
            allowedProtocols.ValueOrDefault(this.allowedProtocols)
        );
    }
};

internal class CssStyle : ICssStyle
{
    public string? PaddingRight { get; set; }
    public string? PaddingTop { get; set; }
    public string? PaddingBottom { get; set; }
    public string? MarginBottom { get; set; }
    public IReactCSSProperties_pointerEvents? PointerEvents { get; set; }
    public string? BorderBottomWidth { get; set; }
    public string? BorderColor { get; set; }
    public string? BorderRightWidth { get; set; }
    public string? BorderTopWidth { get; set; }
    public string? Bottom { get; set; }
    public string? Color { get; set; }
    public string? Height { get; set; }
    public string? Left { get; set; }
    public string? MarginLeft { get; set; }
    public string? MarginRight { get; set; }
    public string? MarginTop { get; set; }
    public string? MinWidth { get; set; }
    public string? PaddingLeft { get; set; }
    public string? Position { get; set; }
    public string? Top { get; set; }
    public string? Width { get; set; }
    public string? VerticalAlign { get; set; }

    /// <summary> Ctor for JsonSerializer. </summary>
    public CssStyle() { }
    public CssStyle(string? paddingRight,
                        string? paddingTop,
                        string? paddingBottom,
                        string? marginBottom,
                        IReactCSSProperties_pointerEvents? pointerEvents,
                        string? borderBottomWidth,
                        string? borderColor,
                        string? borderRightWidth,
                        string? borderTopWidth,
                        string? bottom,
                        string? color,
                        string? height,
                        string? left,
                        string? marginLeft,
                        string? marginRight,
                        string? marginTop,
                        string? minWidth,
                        string? paddingLeft,
                        string? position,
                        string? top,
                        string? width,
                        string? verticalAlign)
    {
        this.PaddingRight = paddingRight;
        this.PaddingTop = paddingTop;
        this.PaddingBottom = paddingBottom;
        this.MarginBottom = marginBottom;
        this.PointerEvents = pointerEvents;
        this.BorderBottomWidth = borderBottomWidth;
        this.BorderColor = borderColor;
        this.BorderRightWidth = borderRightWidth;
        this.BorderTopWidth = borderTopWidth;
        this.Bottom = bottom;
        this.Color = color;
        this.Height = height;
        this.Left = left;
        this.MarginLeft = marginLeft;
        this.MarginRight = marginRight;
        this.MarginTop = marginTop;
        this.MinWidth = minWidth;
        this.PaddingLeft = paddingLeft;
        this.Position = position;
        this.Top = top;
        this.Width = width;
        this.VerticalAlign = verticalAlign;
    }


    public override bool Equals(object? obj)
    {
        return this.Equals(obj as ICssStyle);
    }
    protected bool Equals([NotNullWhen(true)] ICssStyle? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(other, this)) // optimization
            return true;

        if (this.PaddingRight != other.PaddingRight)
            return false;
        if (this.PaddingTop != other.PaddingTop)
            return false;
        if (this.PaddingBottom != other.PaddingBottom)
            return false;
        if (this.MarginBottom != other.MarginBottom)
            return false;
        if (this.PointerEvents?.Equals(other.PointerEvents) ?? other.PointerEvents is { })
            return false;
        if (this.BorderBottomWidth != other.BorderBottomWidth)
            return false;
        if (this.BorderColor != other.BorderColor)
            return false;
        if (this.BorderRightWidth != other.BorderRightWidth)
            return false;
        if (this.BorderTopWidth != other.BorderTopWidth)
            return false;
        if (this.Bottom != other.Bottom)
            return false;
        if (this.Color != other.Color)
            return false;
        if (this.Height != other.Height)
            return false;
        if (this.Left != other.Left)
            return false;
        if (this.MarginLeft != other.MarginLeft)
            return false;
        if (this.MarginRight != other.MarginRight)
            return false;
        if (this.MarginTop != other.MarginTop)
            return false;
        if (this.MinWidth != other.MinWidth)
            return false;
        if (this.PaddingLeft != other.PaddingLeft)
            return false;
        if (this.Position != other.Position)
            return false;
        if (this.Top != other.Top)
            return false;
        if (this.Width != other.Width)
            return false;
        if (this.VerticalAlign != other.VerticalAlign)
            return false;
        return true;
    }
    public override int GetHashCode() => throw new NotImplementedException();


    public ICssStyle With(Option<string?> paddingRight = default,
                          Option<string?> paddingTop = default,
                          Option<string?> paddingBottom = default,
                          Option<string?> marginBottom = default,
                          Option<IReactCSSProperties_pointerEvents?> pointerEvents = default,
                          Option<string?> borderBottomWidth = default,
                          Option<string?> borderColor = default,
                          Option<string?> borderRightWidth = default,
                          Option<string?> borderTopWidth = default,
                          Option<string?> bottom = default,
                          Option<string?> color = default,
                          Option<string?> height = default,
                          Option<string?> left = default,
                          Option<string?> marginLeft = default,
                          Option<string?> marginRight = default,
                          Option<string?> marginTop = default,
                          Option<string?> minWidth = default,
                          Option<string?> paddingLeft = default,
                          Option<string?> position = default,
                          Option<string?> top = default,
                          Option<string?> width = default,
                          Option<string?> verticalAlign = default)
    {
        return new CssStyle(
            paddingRight.ValueOrDefault(this.PaddingRight),
            paddingTop.ValueOrDefault(this.PaddingTop),
            paddingBottom.ValueOrDefault(this.PaddingBottom),
            marginBottom.ValueOrDefault(this.MarginBottom),
            pointerEvents.ValueOrDefault(this.PointerEvents),
            borderBottomWidth.ValueOrDefault(this.BorderBottomWidth),
            borderColor.ValueOrDefault(this.BorderColor),
            borderRightWidth.ValueOrDefault(this.BorderRightWidth),
            borderTopWidth.ValueOrDefault(this.BorderTopWidth),
            bottom.ValueOrDefault(this.Bottom),
            color.ValueOrDefault(this.Color),
            height.ValueOrDefault(this.Height),
            left.ValueOrDefault(this.Left),
            marginLeft.ValueOrDefault(this.MarginLeft),
            marginRight.ValueOrDefault(this.MarginRight),
            marginTop.ValueOrDefault(this.MarginTop),
            minWidth.ValueOrDefault(this.MinWidth),
            paddingLeft.ValueOrDefault(this.PaddingLeft),
            position.ValueOrDefault(this.Position),
            top.ValueOrDefault(this.Top),
            width.ValueOrDefault(this.Width),
            verticalAlign.ValueOrDefault(this.VerticalAlign)
        );
    }
}

internal class SourceLocation : ISourceLocation
{
    public class JsonConverter : JsonConverter<ISourceLocation>
    {
        public static JsonConverter Instance { get; } = new JsonConverter();
        private static readonly Regex expectedFormat = new Regex("[0-9]+,[0-9]+");
        public override ISourceLocation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var _ = new StackoverflowDetector(this, reader, typeToConvert);

            if (reader.TokenType == JsonTokenType.String)
            {
                string? s = JsonSerializer.Deserialize<string>(ref reader, options);
                if (string.IsNullOrWhiteSpace(s))
                {
                    throw new JsonException("Unexpected empty string as SourceLocation");
                }
                else if (!expectedFormat.IsMatch(s))
                {
                    throw new JsonException($"Unexpected source location format: '{s}'");
                }
                else
                {
                    int separatorIndex = s.IndexOf(",");
                    int start = int.Parse(s[..separatorIndex]);
                    int end = int.Parse(s[(separatorIndex + 1)..]);
                    return new SourceLocation(start, end);
                }
            }
            else
            {
                int? start = null;
                int? end = null;
                Contract.Assert(reader.TokenType == JsonTokenType.StartObject);
                reader.Read();

                do
                {
                    Contract.Assert(reader.TokenType == JsonTokenType.PropertyName);
                    string? name = reader.GetString();
                    reader.Read();
                    switch (name)
                    {
                        case "lexer":
                            // ignore the lexer:
                            reader.GetTokenAsJson();
                            break;
                        case "start":
                            start = reader.GetInt32();
                            break;
                        case "end":
                            end = reader.GetInt32();
                            break;
                        default:
                            throw new JsonException($"Unexpected property '{name}' in SourceLocation");
                    }
                    reader.Read();
                } while (reader.TokenType != JsonTokenType.EndObject);
                if (start == null)
                    throw new JsonException("Invalid SourceLocation: 'start' not found");
                else if (end == null)
                    throw new JsonException("Invalid SourceLocation: 'end' not found");
                else
                    return new SourceLocation(start.Value, end.Value);
            }
        }

        public override void Write(Utf8JsonWriter writer, ISourceLocation value, JsonSerializerOptions options)
        {
            Contract.Requires(value != null);
            Contract.Requires(value.Start >= 0);
            Contract.Requires(value.End >= 0);
            writer.WriteStringValue($"{value.Start},{value.End}");
        }
        private JsonConverter() { }
    }

    public required int start { get; init; }
    public required int end { get; init; }


    [SetsRequiredMembers]
    public SourceLocation(int start, int end)
    {
        this.start = start;
        this.end = end;
    }
    public virtual ISourceLocation With(Option<int> start = default,
                                        Option<int> end = default)
    {
        return new SourceLocation(start.ValueOrDefault(this.start),
                                    end.ValueOrDefault(this.end));
    }

    public override bool Equals(object? obj)
    {
        return this.Equals(obj as ISourceLocation);
    }

    protected bool Equals([NotNullWhen(true)] ISourceLocation? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(other, this)) // optimization
            return true;

        if (this.start != other.Start)
            return false;
        if (this.end != other.End)
            return false;
        return true;
    }
    public override int GetHashCode() => throw new NotImplementedException();

    [DebuggerHidden]
    int ISourceLocation.Start => start;
    [DebuggerHidden]
    int ISourceLocation.End => end;

}

// currently sealed because extension is not supported, but may be opened up later
internal sealed class Attributes : ReadOnlyDictionary<string, object?>, IAttributes
{
    public class JsonConverter : DictionaryJsonConverter<object?, IAttributes>
    {
        public static JsonConverter Instance { get; } = new JsonConverter();
        static IAttributes ctor(Dictionary<string, object?> deserialized)
        {
            return new Attributes(deserialized);
        }
        static IReadOnlyDictionary<string, Type?> elementTypes => new Dictionary<string, Type?>
        {
            // the types are the key type for json deserialization. So they trigger which jsonConverter to use
            { sourceLocationKey, typeof(ISourceLocation) },
        };
        private JsonConverter() : base(ctor, elementTypes: elementTypes) { }
        public override IAttributes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // base call already detected stackoverflows

            return base.Read(ref reader, typeToConvert, options);
        }
    }



    public Attributes(IReadOnlyDictionary<string, object?> underlying)
        : base(underlying)
    {
        if (underlying.TryGetValue(sourceLocationKey, out object? sourceLocation))
            Contract.Requires(sourceLocation is ISourceLocation, $"If '{sourceLocationKey}' is specified, it must be of type {nameof(SourceLocation)}");
    }

    public ISourceLocation? SourceLocation
    {
        get
        {
            return underlying.GetValueOrDefault(sourceLocationKey, null) as ISourceLocation;
        }
    }
    private const string sourceLocationKey = "data-loc";


    public /*virtual*/ IAttributes With(params KeyValuePair<string, object?>[] newProperties)
    {
        if (newProperties.Length == 0)
            return this;

        var newUnderlying = new Dictionary<string, object?>(this.underlying);
        foreach (var kvp in newProperties)
            newUnderlying[kvp.Key] = kvp.Value;

        return new Attributes(newUnderlying);
    }
}
