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

	private async Task<IHtmlDomNode> Render(char c)
	{
		var domNode = await KaTeX.RenderToDom(new string(c, 1));
		Contract.Assert(domNode != null);
		Contract.Assert(domNode.Children != null);
		Contract.Assert(domNode.Children.Count == 2);

		var child = domNode.Children[1];
		return child;
	}

	[Fact]
	public async Task SingleCharacterRenderToDomContainsSourceLocation()
	{
		var child = await Render('c');

		var grandchild = child.Children[0];
		var greatgrandchild = grandchild.Children[1];

		Contract.Assert("true".Equals(child.Attributes["aria-hidden"]));
		Contract.Assert(new SourceLocation(0, 1).Equals(greatgrandchild.Attributes.SourceLocation));
	}

	[Fact]
	public async Task SingleCharacterRenderToDomContainsTag()
	{
		IHtmlDomNode child = await Render('c');

		Contract.Assert(child.Tag == "span");
	}
}
