using System.Collections.ObjectModel;

namespace BlaTeX.Tests.Mocks;


/// <summary>
/// A KaTeX runtime for which all operations are ignored in test comparison with diff:ignore.
/// </summary>
record MockKaTeXRuntime : IKaTeXRuntime
{
    public string RenderToStringReturnValue { get; init; } = "<div diff:ignore></div>";
    public IReadOnlyList<IAnyParseNode> ParseReturnValue { get; init; } = DummyParseNodeList.Instance;


    public MockKaTeXRuntime With(Option<string> renderToStringReturnValue = default,
                                 Option<IReadOnlyList<IAnyParseNode>> parseReturnValue = default)
    {
        var @this = this;
        if (renderToStringReturnValue.HasValue)
            @this = @this with { RenderToStringReturnValue = renderToStringReturnValue.Value };
        if (parseReturnValue.HasValue)
            @this = @this with { ParseReturnValue = parseReturnValue.Value };
        return @this;
    }

    public Task<IReadOnlyList<IAnyParseNode>> Parse(string math) => Task.FromResult(ParseReturnValue);
    public Task<string> RenderToString(string math) => Task.FromResult(RenderToStringReturnValue);
    public Task<string> RenderToString(IReadOnlyList<IAnyParseNode> tree, string? math = null) => Task.FromResult(RenderToStringReturnValue);
    public Task<string> ToMarkup(IVirtualNode node) => Task.FromResult(RenderToStringReturnValue);

    public Task<IHtmlDomNode> RenderToDom(string math) => throw new NotImplementedException();

    class DummyParseNodeList : ReadOnlyCollection<IAnyParseNode>
    {
        public static DummyParseNodeList Instance = new();
        private DummyParseNodeList() : base(Array.Empty<IAnyParseNode>()) { }
    }
}
