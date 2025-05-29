using BlaTeX.JSInterop.KaTeX.Internal;

namespace BlaTeX.Tests;

public class KaTeXTypesSerializationTests
{
    private NodeJSRuntime jsRuntime;
    private readonly IKaTeXRuntime KaTeX;
    public KaTeXTypesSerializationTests()
    {
        jsRuntime = NodeJSRuntime.CreateDefault();
        KaTeX = IKaTeXRuntime.Create(jsRuntime);
    }

    [Fact]
    public async Task SerializeSqrtParseNode()
    {
        var node = new AnyParseNode(NodeType.Sqrt, Mode.Math, new SourceLocation(0, 4));
        string s = await KaTeX.RenderToString(new[] { node });
        Contract.Assert(s != null);
    }
}
