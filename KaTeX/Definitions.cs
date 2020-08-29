#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using JBSnorro;
using JBSnorro.Extensions;
using JBSnorro.Text.Json;
using System.Text.Json;

namespace BlaTeX.JSInterop.KaTeX
{
	public interface HtmlDomNode : VirtualNode
	{
		public static HtmlDomNode Create(
			IReadOnlyList<string> classes,
			float height,
			float depth,
			float maxFontSize,
			CssStyle style)
		{
			return new _HtmlDomNode(classes, height, depth, maxFontSize, (_CssStyle)style);
		}
		IReadOnlyList<string> Classes { get; }
		// either of type int or float
		float Height { get; }
		float Depth { get; }
		float MaxFontSize { get; }
		CssStyle Style { get; }
		HtmlDomNode With(Option<IReadOnlyList<string>> classes = default,
						 Option<float> height = default,
						 Option<float> depth = default,
						 Option<float> maxFontSize = default,
						 Option<CssStyle> style = default);
	}


	public interface CssStyle
	{
		// blatex-added properties:
		string? PaddingRight { get; }
		// paddingLeft already exist among the originals
		string? PaddingTop { get; }
		string? PaddingBottom { get; }
		string? MarginBottom { get; }
		ReactCSSProperties_pointerEvents? PointerEvents { get; }
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

		CssStyle With(Option<string?> paddingRight = default,
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
					  Option<string?> verticalAlign = default);
	}

	public interface ReactCSSProperties_pointerEvents
	{
	}

	public interface Span<TChildNode> : HtmlDomNode where TChildNode : VirtualNode
	{
		// Span<T> is not part of the polymorphic deserializers because it's indistinguishable from their direct descendants (DomSpan for now only)
		public IReadOnlyList<TChildNode> Children { get; }
		public Attributes Attributes { get; }
		public float? Width { get; }

		Span<TChildNode> With(Option<TChildNode[]> children = default,
							  Option<Attributes> attributes = default,
							  Option<float?> width = default,
							  Option<IReadOnlyList<string>> classes = default,
							  Option<float> height = default,
							  Option<float> depth = default,
							  Option<float> maxFontSize = default,
							  Option<CssStyle> style = default);
	}
	public interface DomSpan : Span<HtmlDomNode>
	{
		new DomSpan With(Option<HtmlDomNode[]> children = default,
						 Option<Attributes> attributes = default,
						 Option<float?> width = default,
						 Option<IReadOnlyList<string>> classes = default,
						 Option<float> height = default,
						 Option<float> depth = default,
						 Option<float> maxFontSize = default,
						 Option<CssStyle> style = default)
		{
			return (DomSpan)((Span<HtmlDomNode>)this).With(children, attributes, width, classes, height, depth, maxFontSize, style);
		}
	}


	public interface VirtualNode
	{
		/// <summary> Convert into an HTML node. </summary>
		Node ToNode() => throw new NotImplementedException(); // Task<Node> ToNode(IKaTeX runtime) => runtime.ToNode(this);
		/// <summary> Convert into an HTML markup string. </summary>
		Task<string> ToMarkup(IKaTeX runtime) => runtime.ToMarkup(this);
	}
	public interface MathDomNode : VirtualNode
	{
		string toText();
	}
	/**
     * This node represents a general purpose MathML node of any type. The
     * constructor requires the type of node to create (for example, `"mo"` or
     * `"mspace"`, corresponding to `<mo>` and `<mspace>` tags).
     */
	interface MathNode : MathDomNode
	{
		MathNodeType Type { get; }
		IReadOnlyDictionary<string, string> Attributes { get; }
		IReadOnlyList<MathDomNode> Children { get; }

		// public ctor(type: MathNodeType, children?: MathDomNode[]);
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
	interface SettingsOptions
	{
		bool? DisplayMode { get; }
		bool? ThrowOnError { get; }
		string? ErrorColor { get; }
		MacroMap? Macros { get; }
		bool? ColorIsTextColor { get; }
		Strict? Strict { get; }
		float? MaxSize { get; }
		float? MaxExpand { get; }
		IReadOnlyList<string>? AllowedProtocols { get; }

		public SettingsOptions With(Option<bool?> displayMode = default,
									Option<bool?> throwOnError = default,
									Option<string?> errorColor = default,
									Option<MacroMap?> macros = default,
									Option<bool?> colorIsTextColor = default,
									Option<Strict?> strict = default,
									Option<float?> maxSize = default,
									Option<float?> maxExpand = default,
									Option<string[]?> allowedProtocols = default);
	};

	interface MacroMap
	{
		// not implemented
	}
	interface Strict
	{
		// not implemented.
		// export type = boolean | "ignore" | "warn" | "error" | StrictFunction
		// export type StrictFunction = (errorCode: string, errorMsg: string, token?: Token | AnyParseNode) => boolean | string | undefined;
	}
	public interface Attributes : IReadOnlyDictionary<string, object?>
	{
		SourceLocation? SourceLocation { get; }

	}
	public interface SourceLocation
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

	namespace Syntax
	{
		/// <summary> Follows KaTeX naming convention. </summary>
		public interface AnyParseNode
		{
			NodeType Type { get; }
			Mode Mode { get; }
			SourceLocation? SourceLocation { get; }
		}
		public interface BlaTeXNode : AnyParseNode
		{
			string Key { get; }
			string? Arg { get; }
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
			/// <summary> Gets the type (subtype of <see cref="AnyParseNode">) that represents the specified node type. </summary>
			public static Type GetASTType(this NodeType type)
			{
				switch (type)
				{
					case NodeType.Blatex:
						return typeof(BlaTeXNode);
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

	}
}
namespace BlaTeX.JSInterop
{
	public interface Node
	{
	}

}