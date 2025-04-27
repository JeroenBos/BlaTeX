using System.Diagnostics;
using BlaTeX.JSInterop.KaTeX.Syntax;

namespace BlaTeX.JSInterop.KaTeX;

public interface IKaTeXRuntime
{
    public static IKaTeXRuntime Create(IJSRuntime jsRuntime)
    {
        if (jsRuntime == null)
            throw new ArgumentNullException(nameof(jsRuntime));

        return new KaTeXRuntime(jsRuntime);
    }
    Task<string> RenderToString(string math);
    Task<HtmlDomNode> RenderToDom(string math);
    Task<string> ToMarkup(VirtualNode node);
    Task<IReadOnlyList<AnyParseNode>> Parse(string math);
    Task<string> RenderToString(IReadOnlyList<AnyParseNode> tree, string? math = null);
    Task<string> RenderToString(AnyParseNode node, string? math = null) => RenderToString(new[] { node }, math);
}

internal class KaTeXRuntime : IKaTeXRuntime
{
    private readonly IJSRuntime jsRuntime;

    [DebuggerHidden]
    public KaTeXRuntime(IJSRuntime jsRuntime)
    {
        if (jsRuntime == null)
            throw new ArgumentNullException(nameof(jsRuntime));

        this.jsRuntime = jsRuntime;
    }

    public async Task<string> RenderToString(string math)
    {
        Contract.Requires(math != null, nameof(math));

        var result = await InvokeAsync<string>("renderToString", arguments: math);
        Contract.Ensures(result != null);
        return result;
    }
    public async Task<string> RenderToString(IReadOnlyList<AnyParseNode> tree, string? math = null)
    {
        Contract.Requires(tree != null, nameof(tree));
        Contract.RequiresForAll(tree, node => node != null);

        var result = await InvokeAsync<string>("__renderTreeToString", (object?)tree, math);
        Contract.Ensures(result != null);
        return result;
    }

    public async Task<HtmlDomNode> RenderToDom(string math)
    {
        Contract.Requires(math != null, nameof(math));

        var result = await InvokeAsync<HtmlDomNode>("__renderToDomTree", arguments: math);
        Contract.Ensures(result != null);
        return result;
    }
    public async Task<IReadOnlyList<AnyParseNode>> Parse(string math)
    {
        Contract.Requires(math != null, nameof(math));

        var result = await InvokeAsync<AnyParseNode[]>("__parse", arguments: math);
        Contract.Ensures(result != null);
        Contract.Ensures(result.All(node => node != null));
        return result;
    }

    /// <summary> Converters the specified tree to KaTeX's dom representation. <summary>
    /// <param name="tree"> The tree to render. </param>
    /// <param name="math"> If omitted, no MathML will be generated. </param>
    public async Task<DomSpan> RenderToDom(AnyParseNode[] tree, string? math = null)
    {
        Contract.Requires(tree != null, nameof(tree));
        Contract.RequiresForAll(tree, node => node != null);

        var result = await InvokeAsync<DomSpan>("__parseToDomTree", tree, math);
        Contract.Ensures(result != null);
        return result;
    }
    public async Task<string> ToMarkup(VirtualNode node)
    {
        Contract.Requires(node != null, nameof(node));

        var result = await InvokeAsync<string>(node, "toMarkup");
        Contract.Ensures(result != null);
        return result;
    }

    private Task<T> InvokeAsync<T>(string name, params object?[] arguments)
    {
        if (string.IsNullOrWhiteSpace(name))
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            else
                throw new ArgumentException("String empty", nameof(name));
        if (arguments == null)
            throw new ArgumentNullException(nameof(arguments));

        arguments = arguments.Map(arg => arg is null ? JSSourceCode.Null : arg);

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
