global using IDomSpan = BlaTeX.JSInterop.KaTeX.ISpan<BlaTeX.JSInterop.KaTeX.IHtmlDomNode>;

using System.Text.Json.Serialization;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace BlaTeX.JSInterop.KaTeX;

public interface IVirtualNode
{
    /// <summary> Convert into an HTML markup string. </summary>
    Task<string> ToMarkup(IKaTeXRuntime runtime) => runtime.ToMarkup(this);
    /// <summary>
    /// Gets the Html tag of this node.
    /// </summary>
    string Tag { get; }
    // also has a function ToNode(..), which converts to html node, which doesn't seem useful from interop.
}

public interface IMathDomNode : IVirtualNode
{
    string ToText();
}
/// <summary>
/// This node represents a general purpose MathML node of any type.
/// The constructor requires the type of node to create (for example, "mo"` or `"mspace"`, corresponding to `<mo>` and `<mspace>` tags).
/// </summary>
interface IMathNode : IMathDomNode
{
    MathNodeType Type { get; }
    IReadOnlyDictionary<string, string> Attributes { get; }
    IReadOnlyList<IMathDomNode> Children { get; }

    // public ctor(type: MathNodeType, children?: MathDomNode[]);
}

public interface IHtmlDomNode : IVirtualNode
{
    IReadOnlyList<string>? Classes { get; }
    // either of type int or float
    float Height { get; }
    float Depth { get; }
    float MaxFontSize { get; }
    ICssStyle? Style { get; }
    IHtmlDomNode With(Option<IReadOnlyList<string>> classes = default,
                      Option<float> height = default,
                      Option<float> depth = default,
                      Option<float> maxFontSize = default,
                      Option<ICssStyle> style = default);
}

/// <summary>
/// In KaTeX the following aliases exist:
/// - DomSpan = Span<HtmlDomNode>
/// - SvgSpan = Span<SvgNode>
/// 
/// This node represents a span node, with a className, a list of children, and
/// an inline style. It also contains information about its height, depth, and
/// maxFontSize.
///
/// Represents two types with different uses: SvgSpan to wrap an SVG and DomSpan
/// otherwise. This typesafety is important when HTML builders access a span's
/// children.
/// </summary>
/// <typeparam name="TChildNode"></typeparam>.
public interface ISpan<TChildNode> : IHtmlDomNode where TChildNode : IVirtualNode
{
    // Span<T> is not part of the polymorphic deserializers because it's indistinguishable from their direct descendants (DomSpan for now only)
    public IReadOnlyList<TChildNode> Children { get; }
    public IAttributes Attributes { get; }
    public float? Width { get; }

    ISpan<TChildNode> With(Option<TChildNode[]> children = default,
                            Option<IAttributes> attributes = default,
                            Option<float?> width = default,
                            Option<IReadOnlyList<string>> classes = default,
                            Option<float> height = default,
                            Option<float> depth = default,
                            Option<float> maxFontSize = default,
                            Option<ICssStyle> style = default);
}

/// <summary>
/// This node represents an anchor (<a>) element with a hyperlink. See `span` for further details.
/// </summary>
public interface IAnchor : IHtmlDomNode
{
}
/// <summary>
/// A symbol node contains information about a single symbol. It either renders
/// to a single text node, or a span with a single text node in it, depending on
/// whether it has CSS classes, styles, or needs italic correction.
/// </summary>
public interface ISymbolNode : IHtmlDomNode
{
}
public interface ISvgNode : IVirtualNode
{
    public interface IPathNode : IVirtualNode
    {
        // child node of SvgNode
    }
    public interface ILineNode : IVirtualNode
    {
        // child node of SvgNode
    }
}
/// <summary>
/// This node represents an image embed (<img>) element.
/// </summary>
public interface IImageNode : IVirtualNode
{
}

/// <summary> Follows KaTeX naming convention. </summary>
public interface IAnyParseNode
{
    NodeType Type { get; }
    Mode Mode { get; }
    ISourceLocation? SourceLocation { get; }
}
public interface IBlaTeXNode : IAnyParseNode
{
    IAnyParseNode[] Args { get; }
}

public enum MathNodeType
{
    None,
    math,
    annotation,
    semantics,
    mtext,
    mn,
    mo,
    mi,
    mspace,
    mover,
    munder,
    munderover,
    msup,
    msub,
    msubsup,
    mfrac,
    mroot,
    msqrt,
    mtable,
    mtr,
    mtd,
    mlabeledtr,
    mrow,
    menclose,
    mstyle,
    mpadded,
    mphantom,
    mglyph,
}

interface ISettingsOptions
{
    bool? DisplayMode { get; }
    bool? ThrowOnError { get; }
    string? ErrorColor { get; }
    IMacroMap? Macros { get; }
    bool? ColorIsTextColor { get; }
    IStrict? Strict { get; }
    float? MaxSize { get; }
    float? MaxExpand { get; }
    IReadOnlyList<string>? AllowedProtocols { get; }

    public ISettingsOptions With(Option<bool?> displayMode = default,
                                Option<bool?> throwOnError = default,
                                Option<string?> errorColor = default,
                                Option<IMacroMap?> macros = default,
                                Option<bool?> colorIsTextColor = default,
                                Option<IStrict?> strict = default,
                                Option<float?> maxSize = default,
                                Option<float?> maxExpand = default,
                                Option<string[]?> allowedProtocols = default);
};

interface IMacroMap
{
    // not implemented
}
interface IStrict
{
    // not implemented.
    // export type = boolean | "ignore" | "warn" | "error" | StrictFunction
    // export type StrictFunction = (errorCode: string, errorMsg: string, token?: Token | IAnyParseNode) => boolean | string | undefined;
}

public interface IAttributes : IReadOnlyDictionary<string, object?>
{
    ISourceLocation? SourceLocation { get; }

    public static IAttributes Empty { get; } = new EmptyAttributes();
    private sealed class EmptyAttributes : IAttributes
    {
        public object? this[string key] => throw new KeyNotFoundException(key);
        public ISourceLocation? SourceLocation => null;
        public IEnumerable<string> Keys => [];
        public IEnumerable<object?> Values => [];
        public int Count => 0;
        public bool ContainsKey(string key) => false;
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            yield break;
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
        {
            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

public interface ISourceLocation
{
    int Start { get; }
    int End { get; }
    int Length => End - Start;
}

public enum Mode
{
    Unknown,
    Math,
    Text,
}
public static class ModeExtensions
{
    public static JsonConverter<Mode> Instance { get; } = new LowerCaseEnumStringJsonConverter<Mode>();
}

/// <summary> Follows KaTeX naming convention. </summary>
public enum NodeType
{
    Unknown,
    Blatex,
    Array,
    /// <summary> "color-token" </summry>
    ColorToken,
    Op,
    Ordgroup,
    Raw,
    Size,
    Styling,
    Supsub,
    Tag,
    Text,
    Url,
    Verb,
    Atom,
    Mathord,
    Spacing,
    Textord,
    /// <summary> "accent-token" </summary>
    AccentToken,
    /// <summary> "op-token" </summary>
    OpToken,
    Accent,
    AccentUnder,
    Cr,
    Delimsizing,
    Enclose,
    Environment,
    Font,
    Genfrac,
    HorizBrace,
    Href,
    Html,
    HtmlMathml,
    IncludeGraphics,
    Infix,
    Internal,
    Kern,
    Lap,
    LeftRight,
    /// <summary> "leftright-right"</summary>
    LeftRightLeft,
    MathChoice,
    Middle,
    Mclass,
    OperatorName,
    Overline,
    Phantom,
    HPhantom,
    VPhantom,
    RaiseBox,
    Rule,
    Sizing,
    Smash,
    Sqrt,
    Underline,
    XArrow,
}
public static class NodeTypeExtensions
{
    /// <summary> Gets the type (subtype of <see cref="IAnyParseNode">) that represents the specified node type. </summary>
    public static Type GetASTType(this NodeType type)
    {
        switch (type)
        {
            case NodeType.Blatex:
                return typeof(IBlaTeXNode);
            case NodeType.Unknown:
            default:
                return null!;  // This means there is no dedicated runtime type for this node type
        }
    }
    public static JsonConverter<NodeType> JsonConverterInstance => NodeTypeJsonConverter.Instance;

    private class NodeTypeJsonConverter : EnumStringJsonConverter<NodeType>
    {
        public static new NodeTypeJsonConverter Instance { get; } = new NodeTypeJsonConverter();
        protected override NodeType Parse(string name)
        {
            return base.Parse(name.Replace("-", ""));
        }

        protected override string GetName(NodeType value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case NodeType.LeftRightLeft:
                    return "leftright-right";
                case NodeType.AccentToken:
                    return "accent-token";
                case NodeType.OpToken:
                    return "op-token";
                case NodeType.ColorToken:
                    return "color-token";
                default:
                    return value.ToString().ToLowerInvariant();
            }
        }

    }
}

public interface ICssStyle
{
    // blatex-added properties:
    string? PaddingRight { get; }
    // paddingLeft already exist among the originals
    string? PaddingTop { get; }
    string? PaddingBottom { get; }
    string? MarginBottom { get; }
    IReactCSSProperties_pointerEvents? PointerEvents { get; }
    // original cssstyle properties:
    string? BorderBottomWidth { get; }
    string? BorderColor { get; }
    string? BorderRightWidth { get; }
    string? BorderTopWidth { get; }
    string? Bottom { get; }
    string? Color { get; }
    string? Height { get; }
    string? Left { get; }
    string? MarginLeft { get; }
    string? MarginRight { get; }
    string? MarginTop { get; }
    string? MinWidth { get; }
    string? PaddingLeft { get; }
    string? Position { get; }
    string? Top { get; }
    string? Width { get; }
    string? VerticalAlign { get; }

    ICssStyle With(Option<string?> paddingRight = default,
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
                    Option<string?> verticalAlign = default);
}

public interface IReactCSSProperties_pointerEvents
{
}
