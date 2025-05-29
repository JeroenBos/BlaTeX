using System.Diagnostics;

namespace BlaTeX.JSInterop.KaTeX.Internal;

internal class KaTeXRuntime : IKaTeXRuntime
{
    private readonly IJSRuntime jsRuntime;

    [DebuggerHidden]
    public KaTeXRuntime(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public async Task<string> RenderToString(string math)
    {
        Contract.Requires(math != null, nameof(math));

        var result = await InvokeAsync<string>("renderToString", arguments: math);
        Contract.Ensures(result != null);
        return result;
    }
    public async Task<string> RenderToString(IReadOnlyList<IAnyParseNode> tree, string? math = null)
    {
        Contract.Requires(tree != null, nameof(tree));
        Contract.RequiresForAll(tree, node => node != null);

        var result = await InvokeAsync<string>("__renderTreeToString", (object?)tree, math);
        Contract.Ensures(result != null);
        return result;
    }

    public async Task<IHtmlDomNode> RenderToDom(string math)
    {
        Contract.Requires(math != null, nameof(math));

        var result = await InvokeAsync<IHtmlDomNode>("__renderToDomTree", arguments: math);
        Contract.Ensures(result != null);
        return result;
    }
    public async Task<IReadOnlyList<IAnyParseNode>> Parse(string math)
    {
        Contract.Requires(math != null, nameof(math));

        var result = await InvokeAsync<IAnyParseNode[]>("__parse", arguments: math);
        Contract.Ensures(result != null);
        Contract.Ensures(result.All(node => node != null));
        return result;
    }

    /// <summary> Converters the specified tree to KaTeX's dom representation. <summary>
    /// <param name="tree"> The tree to render. </param>
    /// <param name="math"> If omitted, no MathML will be generated. </param>
    public async Task<IHtmlDomNode> RenderToDom(IAnyParseNode[] tree, string? math = null)
    {
        Contract.Requires(tree != null, nameof(tree));
        Contract.RequiresForAll(tree, node => node != null);

        var result = await InvokeAsync<IHtmlDomNode>("__parseToDomTree", tree, math);
        Contract.Ensures(result != null);
        return result;
    }
    public async Task<string> ToMarkup(IVirtualNode node)
    {
        Contract.Requires(node != null, nameof(node));

        var result = await InvokeAsync<string>(node, "toMarkup");
        Contract.Ensures(result != null);
        return result;
    }

    private Task<T> InvokeAsync<T>(string name, params object?[] arguments)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(arguments);

        arguments = arguments.Map(arg => arg is null ? JSSourceCode.Null : arg);

        return jsRuntime.InvokeAsync<T>("katex." + name, arguments)
                        .AsTask();
    }

    private Task<T> InvokeAsync<T>(object instance, string methodName, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentException.ThrowIfNullOrEmpty(methodName);
        ArgumentNullException.ThrowIfNull(arguments);

        return jsRuntime.InvokeAsync<T>("katex." + methodName, arguments)
                        .AsTask();
    }
}
