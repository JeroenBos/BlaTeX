namespace BlaTeX.JSInterop.KaTeX;

public interface IKaTeXRuntime
{
    public static IKaTeXRuntime Create(IJSRuntime jsRuntime)
    {
        return new Internal.KaTeXRuntime(jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime)));
    }

    Task<string> RenderToString(string math);
    Task<HtmlDomNode> RenderToDom(string math);
    Task<string> ToMarkup(VirtualNode node);
    Task<IReadOnlyList<AnyParseNode>> Parse(string math);
    Task<string> RenderToString(IReadOnlyList<AnyParseNode> tree, string? math = null);
    Task<string> RenderToString(AnyParseNode node, string? math = null) => RenderToString([node], math);
}
