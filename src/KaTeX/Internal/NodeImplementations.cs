using JBSnorro.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BlaTeX.JSInterop.KaTeX.Internal;

internal class _HtmlDomNode : HtmlDomNode, IJSSerializable
{
    public string SERIALIZATION_TYPE_ID => IJSSerializable.SERIALIZATION_TYPE_ID_Impl(this);
    public IReadOnlyList<string>? classes { get; set; }
    public float height { get; set; }
    public float depth { get; set; }
    public float maxFontSize { get; set; }
    public _CssStyle? style { get; set; }
    [DebuggerHidden]
    IReadOnlyList<string>? HtmlDomNode.Classes => classes;
    [DebuggerHidden]
    float HtmlDomNode.Height => height;
    [DebuggerHidden]
    float HtmlDomNode.Depth => depth;
    [DebuggerHidden]
    float HtmlDomNode.MaxFontSize => maxFontSize;
    [DebuggerHidden]
    CssStyle? HtmlDomNode.Style => style;


    /// <summary> Ctor for JsonSerializer. </summary>
    public _HtmlDomNode() { }
    public _HtmlDomNode(
        IReadOnlyList<string>? classes,
        float height,
        float depth,
        float maxFontSize,
        _CssStyle? style)
    {
        this.classes = classes;
        this.height = height;
        this.depth = depth;
        this.maxFontSize = maxFontSize;
        this.style = style;
    }


    public override bool Equals(object? obj)
    {
        return this.Equals(obj as HtmlDomNode);
    }

    protected bool Equals([NotNullWhen(true)] HtmlDomNode? other)
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

    public virtual HtmlDomNode With(Option<IReadOnlyList<string>> classes = default,
                                    Option<float> height = default,
                                    Option<float> depth = default,
                                    Option<float> maxFontSize = default,
                                    Option<CssStyle> style = default)
    {
        return new _HtmlDomNode(classes.ValueOrDefault(this.classes),
                                height.ValueOrDefault(this.height),
                                depth.ValueOrDefault(this.depth),
                                maxFontSize.ValueOrDefault(this.maxFontSize),
                                (_CssStyle?)style.ValueOrDefault(this.style));
    }
}

/// <summary> A concrete type of Span<T>. </summary>
internal class _DomSpan : _Span<HtmlDomNode>, DomSpan
{
    /// <summary> Ctor for JsonSerializer. </summary>
    public _DomSpan()
    {

    }
    public _DomSpan(
        HtmlDomNode[]? children,
        Attributes? attributes,
        float? width,
        IReadOnlyList<string>? classes,
        float height,
        float depth,
        float maxFontSize,
        _CssStyle? style)
        : base(children, attributes, width, classes, height, depth, maxFontSize, style)
    {
    }
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as DomSpan);
    }
    protected bool Equals([NotNullWhen(true)] DomSpan? other)
    {
        return base.Equals(other);
    }
    public override int GetHashCode() => throw new NotImplementedException();

    public sealed override Span<HtmlDomNode> With(Option<HtmlDomNode[]> children = default,
                                                    Option<Attributes> attributes = default,
                                                    Option<float?> width = default,
                                                    Option<IReadOnlyList<string>> classes = default,
                                                    Option<float> height = default,
                                                    Option<float> depth = default,
                                                    Option<float> maxFontSize = default,
                                                    Option<CssStyle> style = default)
    {
        return new _DomSpan(children.ValueOrDefault(this.children),
                            attributes.ValueOrDefault(this.attributes),
                            width.ValueOrDefault(this.width),
                            classes.ValueOrDefault(this.classes),
                            height.ValueOrDefault(this.height),
                            depth.ValueOrDefault(this.depth),
                            maxFontSize.ValueOrDefault(this.maxFontSize),
                            (_CssStyle?)style.ValueOrDefault(this.style));
    }
}

internal class _Span<TChildNode> : _HtmlDomNode, Span<TChildNode> where TChildNode : VirtualNode
{
    public TChildNode[] children { get; set; }
    public Attributes attributes { get; set; }
    public float? width { get; set; }
    [DebuggerHidden]
    IReadOnlyList<TChildNode> Span<TChildNode>.Children => children;
    [DebuggerHidden]
    Attributes Span<TChildNode>.Attributes => attributes;
    [DebuggerHidden]
    float? Span<TChildNode>.Width => width;

    /// <summary> Ctor for JsonSerializer. </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public _Span() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public _Span(TChildNode[]? children,
                 Attributes? attributes,
                 float? width,
                 IReadOnlyList<string>? classes,
                 float height,
                 float depth,
                 float maxFontSize,
                 _CssStyle? style)
        : base(classes, height, depth, maxFontSize, style)
    {
        this.children = children ?? [];
        this.attributes = attributes ?? Attributes.Empty;
        this.width = width;
    }
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Span<TChildNode>);
    }
    protected bool Equals([NotNullWhen(true)] Span<TChildNode>? other)
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
    public sealed override HtmlDomNode With(Option<IReadOnlyList<string>> classes = default,
                                            Option<float> height = default,
                                            Option<float> depth = default,
                                            Option<float> maxFontSize = default,
                                            Option<CssStyle> style = default)
    {
        return With(default, default, default, classes, height, depth, maxFontSize, style);
    }
    public virtual Span<TChildNode> With(Option<TChildNode[]> children = default,
                                         Option<Attributes> attributes = default,
                                         Option<float?> width = default,
                                         Option<IReadOnlyList<string>> classes = default,
                                         Option<float> height = default,
                                         Option<float> depth = default,
                                         Option<float> maxFontSize = default,
                                         Option<CssStyle> style = default)
    {
        return new _Span<TChildNode>(children.ValueOrDefault(this.children),
                                     attributes.ValueOrDefault(this.attributes),
                                     width.ValueOrDefault(this.width),
                                     classes.ValueOrDefault(this.classes),
                                     height.ValueOrDefault(this.height),
                                     depth.ValueOrDefault(this.depth),
                                     maxFontSize.ValueOrDefault(this.maxFontSize),
                                     (_CssStyle?)style.ValueOrDefault(this.style));
    }
}

internal class _SettingsOptions : SettingsOptions
{
    public bool? displayMode { get; set; }
    public bool? throwOnError { get; set; }
    public string? errorColor { get; set; }
    public MacroMap? macros { get; set; }
    public bool? colorIsTextColor { get; set; }
    public Strict? strict { get; set; }
    public float? maxSize { get; set; }
    public float? maxExpand { get; set; }
    public string[]? allowedProtocols { get; set; }

    bool? SettingsOptions.DisplayMode => displayMode;
    bool? SettingsOptions.ThrowOnError => throwOnError;
    string? SettingsOptions.ErrorColor => errorColor;
    MacroMap? SettingsOptions.Macros => macros;
    bool? SettingsOptions.ColorIsTextColor => colorIsTextColor;
    Strict? SettingsOptions.Strict => strict;
    float? SettingsOptions.MaxSize => maxSize;
    float? SettingsOptions.MaxExpand => maxExpand;
    IReadOnlyList<string>? SettingsOptions.AllowedProtocols => allowedProtocols;

    /// <summary> Ctor for JsonSerializer. </summary>
    public _SettingsOptions() { }
    public _SettingsOptions(bool? displayMode,
                            bool? throwOnError,
                            string? errorColor,
                            MacroMap? macros,
                            bool? colorIsTextColor,
                            Strict? strict,
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
        return this.Equals(obj as SettingsOptions);
    }
    protected bool Equals([NotNullWhen(true)] SettingsOptions? other)
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

    public virtual SettingsOptions With(Option<bool?> displayMode = default,
                                        Option<bool?> throwOnError = default,
                                        Option<string?> errorColor = default,
                                        Option<MacroMap?> macros = default,
                                        Option<bool?> colorIsTextColor = default,
                                        Option<Strict?> strict = default,
                                        Option<float?> maxSize = default,
                                        Option<float?> maxExpand = default,
                                        Option<string[]?> allowedProtocols = default)
    {
        return new _SettingsOptions(
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

internal class _CssStyle : CssStyle
{
    public string? PaddingRight { get; set; }
    public string? PaddingTop { get; set; }
    public string? PaddingBottom { get; set; }
    public string? MarginBottom { get; set; }
    public ReactCSSProperties_pointerEvents? PointerEvents { get; set; }
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
    public _CssStyle() { }
    public _CssStyle(string? paddingRight,
                        string? paddingTop,
                        string? paddingBottom,
                        string? marginBottom,
                        ReactCSSProperties_pointerEvents? pointerEvents,
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
        return this.Equals(obj as CssStyle);
    }
    protected bool Equals([NotNullWhen(true)] CssStyle? other)
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


    public CssStyle With(Option<string?> paddingRight = default,
                            Option<string?> paddingTop = default,
                            Option<string?> paddingBottom = default,
                            Option<string?> marginBottom = default,
                            Option<ReactCSSProperties_pointerEvents?> pointerEvents = default,
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
        return new _CssStyle(
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

internal class _SourceLocation : SourceLocation
{
    public class JsonConverter : JsonConverter<SourceLocation>
    {
        public static JsonConverter Instance { get; } = new JsonConverter();
        private static readonly Regex expectedFormat = new Regex("[0-9]+,[0-9]+");
        public override SourceLocation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                    return new _SourceLocation(start, end);
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
                    return new _SourceLocation(start.Value, end.Value);
            }
        }

        public override void Write(Utf8JsonWriter writer, SourceLocation value, JsonSerializerOptions options)
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
    public _SourceLocation(int start, int end)
    {
        this.start = start;
        this.end = end;
    }
    public virtual SourceLocation With(Option<int> start = default,
                                        Option<int> end = default)
    {
        return new _SourceLocation(start.ValueOrDefault(this.start),
                                    end.ValueOrDefault(this.end));
    }

    public override bool Equals(object? obj)
    {
        return this.Equals(obj as SourceLocation);
    }

    protected bool Equals([NotNullWhen(true)] SourceLocation? other)
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
    int SourceLocation.Start => start;
    [DebuggerHidden]
    int SourceLocation.End => end;

}

// currently sealed because extension is not supported, but may be opened up later
internal sealed class _Attributes : ReadOnlyDictionary<string, object?>, Attributes
{
    public class JsonConverter : DictionaryJsonConverter<object?, Attributes>
    {
        public static JsonConverter Instance { get; } = new JsonConverter();
        static Attributes ctor(Dictionary<string, object?> deserialized)
        {
            return new _Attributes(deserialized);
        }
        static IReadOnlyDictionary<string, Type?> elementTypes => new Dictionary<string, Type?>
        {
            // the types are the key type for json deserialization. So they trigger which jsonConverter to use
            { sourceLocationKey, typeof(SourceLocation) },
        };
        private JsonConverter() : base(ctor, elementTypes: elementTypes) { }
        public override Attributes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // base call already detected stackoverflows

            return base.Read(ref reader, typeToConvert, options);
        }
    }



    public _Attributes(IReadOnlyDictionary<string, object?> underlying)
        : base(underlying)
    {
        if (underlying.TryGetValue(sourceLocationKey, out object? sourceLocation))
            Contract.Requires(sourceLocation is SourceLocation, $"If '{sourceLocationKey}' is specified, it must be of type {nameof(SourceLocation)}");
    }

    public SourceLocation? SourceLocation
    {
        get
        {
            return underlying.GetValueOrDefault(sourceLocationKey, null) as SourceLocation;
        }
    }
    private const string sourceLocationKey = "data-loc";


    public /*virtual*/ Attributes With(params KeyValuePair<string, object?>[] newProperties)
    {
        if (newProperties.Length == 0)
            return this;

        var newUnderlying = new Dictionary<string, object?>(this.underlying);
        foreach (var kvp in newProperties)
            newUnderlying[kvp.Key] = kvp.Value;

        return new _Attributes(newUnderlying);
    }
}
