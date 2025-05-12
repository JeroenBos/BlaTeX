global using IDomSpan = BlaTeX.JSInterop.KaTeX.Internal.Span<BlaTeX.JSInterop.KaTeX.IHtmlDomNode>;
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
		var domNode = await KaTeX.RenderToDom("c") as IDomSpan;
		Contract.Assert(domNode != null);
		Contract.Assert(domNode.Children != null);
		Contract.Assert(domNode.Children.Length == 2);

		var child = (IDomSpan)domNode.Children[1];
		var grandchild = (IDomSpan)child.Children[0];
		var greatgrandchild = (IDomSpan)grandchild.Children[1];

		Contract.Assert("true".Equals(child.Attributes["aria-hidden"]));
		Contract.Assert(new SourceLocation(0, 1).Equals(greatgrandchild.Attributes.SourceLocation));
	}
}
