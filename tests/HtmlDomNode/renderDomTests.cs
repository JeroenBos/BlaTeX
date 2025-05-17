using BlaTeX.JSInterop.KaTeX.Internal;

namespace BlaTeX.Tests;

public class HtmlDomNodeTests
{
	private readonly NodeJSRuntime jsRuntime;
	private readonly IKaTeXRuntime KaTeX;
	public HtmlDomNodeTests()
	{
		this.jsRuntime = NodeJSRuntime.CreateDefault();
		this.KaTeX = IKaTeXRuntime.Create(jsRuntime);
	}

	[Fact]
	public async Task SingleCharacterRenderToDomContainsSourceLocation()
    {
		var domNode = await KaTeX.RenderToDom("c");
		Contract.Assert(domNode != null);
		Contract.Assert(domNode.Children != null);
		Contract.Assert(domNode.Children.Count == 2);

		var child = domNode.Children[1];
		var grandchild = child.Children[0];
		var greatgrandchild = grandchild.Children[1];

		Contract.Assert("true".Equals(child.Attributes["aria-hidden"]));
		Contract.Assert(new SourceLocation(0, 1).Equals(greatgrandchild.Attributes.SourceLocation));
	}
}
