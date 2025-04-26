using BlaTeX.JSInterop.KaTeX.Syntax;

namespace BlaTeX;

public interface IChildComponentMarkupService
{
    IEnumerable<Range> Select(string markup);
    RenderFragment Substitute(string markupFragment);
    IReadOnlyList<AnyParseNode> Select(IReadOnlyList<AnyParseNode> ast);
}
