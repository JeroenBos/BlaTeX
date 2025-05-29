namespace BlaTeX.JSInterop.KaTeX;

public interface IKaTeXRuntime
{
    public static IKaTeXRuntime Create(IJSRuntime jsRuntime)
    {
        return new Internal.KaTeXRuntime(jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime)));
    }

    Task<string> RenderToString(string math);
    Task<IHtmlDomNode> RenderToDom(string math);
    Task<string> ToMarkup(IVirtualNode node);
    Task<IReadOnlyList<IAnyParseNode>> Parse(string math);
    Task<string> RenderToString(IReadOnlyList<IAnyParseNode> tree, string? math = null);
    Task<string> RenderToString(IAnyParseNode node, string? math = null) => RenderToString([node], math);
}
