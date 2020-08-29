#nullable enable
using JBSnorro;
using JBSnorro.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlaTeX.JSInterop.KaTeX;
using BlaTeX.JSInterop.KaTeX.Syntax;
using Microsoft.JSInterop;

namespace BlaTeX.JSInterop.KaTeX
{
    public interface IKaTeX
    {
        public static IKaTeX Create(IJSRuntime jsRuntime)
        {
            if (jsRuntime == null)
                throw new ArgumentNullException(nameof(jsRuntime));

            return new _KaTeX(jsRuntime);
        }
        Task<string> RenderToString(string math);
        Task<HtmlDomNode> RenderToDom(string math);
        Task<string> ToMarkup(VirtualNode node);
        Task<AnyParseNode[]> Parse(string math);
        Task<string> RenderToString(IReadOnlyList<AnyParseNode> tree, string? math = null);
        Task<string> RenderToString(AnyParseNode node, string? math = null) => RenderToString(new[] { node }, math);
    }

    internal class _KaTeX : IKaTeX
    {
        private readonly IJSRuntime jsRuntime;

        public _KaTeX(IJSRuntime jsRuntime)
        {
            if (jsRuntime == null)
                throw new ArgumentNullException(nameof(jsRuntime));

            this.jsRuntime = jsRuntime;
        }

        public Task<string> RenderToString(string math) => InvokeAsync<string>("renderToString", arguments: math ?? throw new ArgumentNullException(nameof(math)));
        public Task<string> RenderToString(IReadOnlyList<AnyParseNode> tree, string? math = null) => InvokeAsync<string>("__renderTreeToString", (object?)tree ?? throw new ArgumentNullException(nameof(tree)), math);
        public Task<HtmlDomNode> RenderToDom(string math) => InvokeAsync<HtmlDomNode>("__renderToDomTree", arguments: math ?? throw new ArgumentNullException(nameof(math)));
        public Task<AnyParseNode[]> Parse(string math) => InvokeAsync<AnyParseNode[]>("__parse", arguments: math ?? throw new ArgumentNullException(nameof(math)));
        /// <summary> Converters the specified tree to KaTeX's dom representation. <summary>
        /// <param name="tree"> The tree to render. </param>
        /// <param name="math"> If omitted, no MathML will be generated. </param>
        public Task<DomSpan> RenderToDom(AnyParseNode[] tree, string? math = null) => InvokeAsync<DomSpan>("__parseToDomTree", tree ?? throw new ArgumentNullException(nameof(tree)), math);
        public Task<string> ToMarkup(VirtualNode node) => InvokeAsync<string>(node ?? throw new ArgumentNullException(nameof(node)), "toMarkup");

        private Task<T> InvokeAsync<T>(string name, params object?[] arguments)
        {
            if (string.IsNullOrWhiteSpace(name))
                if (name == null)
                    throw new ArgumentNullException(nameof(name));
                else
                    throw new ArgumentException("String empty", nameof(name));
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            arguments = arguments.Map(arg => arg == null ? JSString.Null : arg);

            return jsRuntime.InvokeAsync<T>("katex." + name, arguments)
                            .AsTask();
        }

        private Task<T> InvokeAsync<T>(object instance, string methodName, params object?[] arguments)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (string.IsNullOrWhiteSpace(methodName))
                if (methodName == null)
                    throw new ArgumentNullException(nameof(methodName));
                else
                    throw new ArgumentException("String empty", nameof(methodName));

            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            return jsRuntime.InvokeAsync<T>("katex." + methodName, arguments)
                            .AsTask();
        }
    }

}