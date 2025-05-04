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
        var node = new _AnyParseNode(NodeType.Sqrt, Mode.Math, new _SourceLocation(0, 4));
        string s = await KaTeX.RenderToString(new[] { node });
        Contract.Assert(s != null);
    }
}
