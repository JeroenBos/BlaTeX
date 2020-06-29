#nullable enable
using JBSnorro;
using JBSnorro.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BlaTeX.JSInterop.KaTeX
{
    internal class _HtmlDomNode : HtmlDomNode
    {
        public IReadOnlyList<string> classes { get; set; } = default!;
        public float height { get; set; } = default!;
        public float depth { get; set; } = default!;
        public float maxFontSize { get; set; } = default!;
        public _CssStyle style { get; set; } = default!;
        [DebuggerHidden]
        IReadOnlyList<string> HtmlDomNode.Classes => classes;
        [DebuggerHidden]
        float HtmlDomNode.Height => height;
        [DebuggerHidden]
        float HtmlDomNode.Depth => depth;
        [DebuggerHidden]
        float HtmlDomNode.MaxFontSize => maxFontSize;
        [DebuggerHidden]
        CssStyle HtmlDomNode.Style => style;


        /// <summary> Ctor for JsonSerializer. </summary>
        public _HtmlDomNode() { }
        public _HtmlDomNode(
            IReadOnlyList<string> classes,
            float height,
            float depth,
            float maxFontSize,
            _CssStyle style)
        {
            this.classes = classes;
            this.height = height;
            this.depth = depth;
            this.maxFontSize = maxFontSize;
            this.style = style;
        }

        Node VirtualNode.ToNode() => throw new NotImplementedException();
        string VirtualNode.ToMarkup() => throw new NotImplementedException();

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
            if (this.classes?.SequenceEqual(other.Classes) ?? other.Classes is { })
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
                                    (_CssStyle)style.ValueOrDefault(this.style));
        }
    }

    /// <summary> A concrete type of Span<T>. </summary>
    internal class _DomSpan : _Span<HtmlDomNode>, DomSpan
    {
        /// <summary> Ctor for JsonSerializer. </summary>
        public _DomSpan() { }
        public _DomSpan(
            HtmlDomNode[] children,
            Dictionary<string, string> attributes,
            float? width,
            IReadOnlyList<string> classes,
            float height,
            float depth,
            float maxFontSize,
            _CssStyle style)
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
                                                      Option<Dictionary<string, string>> attributes = default,
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
                                (_CssStyle)style.ValueOrDefault(this.style));
        }
    }

    internal class _Span<TChildNode> : _HtmlDomNode, Span<TChildNode> where TChildNode : VirtualNode
    {
        public TChildNode[] children { get; set; } = default!;
        public Dictionary<string, string> attributes { get; set; } = default!;
        public float? width { get; set; }
        [DebuggerHidden]
        IReadOnlyList<TChildNode> Span<TChildNode>.Children => children;
        [DebuggerHidden]
        IReadOnlyDictionary<string, string> Span<TChildNode>.Attributes => attributes;
        [DebuggerHidden]
        float? Span<TChildNode>.Width => width;

        /// <summary> Ctor for JsonSerializer. </summary>
        public _Span() { }
        public _Span(TChildNode[] children,
                     Dictionary<string, string> attributes,
                     float? width,
                     IReadOnlyList<string> classes,
                     float height,
                     float depth,
                     float maxFontSize,
                     _CssStyle style)
          : base(classes, height, depth, maxFontSize, style)
        {
            this.children = children;
            this.attributes = attributes;
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
            if (this.children?.SequenceEqual(other.Children) ?? other.Children is { })
                return false;
            if (this.attributes?.Equals(other.Attributes) ?? other.Attributes is { })
                return false;
            return true;
        }
        public override int GetHashCode() => throw new NotImplementedException();
        public sealed override HtmlDomNode With(Option<IReadOnlyList<string>> classes = default(Option<IReadOnlyList<string>>),
                                                Option<float> height = default(Option<float>),
                                                Option<float> depth = default(Option<float>),
                                                Option<float> maxFontSize = default(Option<float>),
                                                Option<CssStyle> style = default(Option<CssStyle>))
        {
            return With(default, default, default, classes, height, depth, maxFontSize, style);
        }
        public virtual Span<TChildNode> With(Option<TChildNode[]> children = default,
                                             Option<Dictionary<string, string>> attributes = default,
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
                                         (_CssStyle)style.ValueOrDefault(this.style));
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
            if (this.allowedProtocols?.SequenceEqual(other.AllowedProtocols) ?? other.AllowedProtocols is { })
                return false;
            return true;
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
}