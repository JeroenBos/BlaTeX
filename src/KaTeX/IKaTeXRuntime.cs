namespace BlaTeX.JSInterop.KaTeX;

public interface IKaTeXRuntime
{
    public static IKaTeXRuntime Create(IJSRuntime jsRuntime)
    {
        if (jsRuntime == null)
            throw new ArgumentNullException(nameof(jsRuntime));

        return new Internal.KaTeXRuntime(jsRuntime);
    }
    Task<string> RenderToString(string math);
    Task<HtmlDomNode> RenderToDom(string math);
    Task<string> ToMarkup(VirtualNode node);
    Task<IReadOnlyList<AnyParseNode>> Parse(string math);
    Task<string> RenderToString(IReadOnlyList<AnyParseNode> tree, string? math = null);
    Task<string> RenderToString(AnyParseNode node, string? math = null) => RenderToString(new[] { node }, math);
}
