#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    }

    internal class _Span<TChildNode> : _HtmlDomNode, Span<TChildNode> where TChildNode : VirtualNode
    {
        public TChildNode[] children { get; set; } = default!;
        public Dictionary<string, string> attributes { get; set; } = default!;
        public float? width { get; set; }
        [DebuggerHidden]
        IReadOnlyList<TChildNode> Span<TChildNode>.Children => children;
        [DebuggerHidden]
        Dictionary<string, string> Span<TChildNode>.Attributes => attributes;
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
    }
}