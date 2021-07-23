#nullable enable
using BlaTeX.JSInterop;
using BlaTeX.JSInterop.KaTeX;
using JBSnorro.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace BlaTeX.Tests
{
	public class HtmlDomNodeTests
	{
		private NodeJSRuntime jsRuntime;
		private readonly IKaTeX KaTeX;
		public HtmlDomNodeTests()
		{
			jsRuntime = NodeJSRuntime.CreateDefault();
			KaTeX = IKaTeX.Create(jsRuntime);
		}

		[Fact]
		public async Task SingleCharacterRenderToDomContainsSourceLocation()
		{
			var domNode = await KaTeX.RenderToDom("c") as DomSpan;
			Contract.Assert(domNode != null);
			Contract.Assert(domNode.Children != null);
			Contract.Assert(domNode.Children.Count == 2);

			var child = (DomSpan)domNode.Children[1];
			var grandchild = (DomSpan)child.Children[0];
			var greatgrandchild = (DomSpan)grandchild.Children[1];

			Contract.Assert("true".Equals(child.Attributes["aria-hidden"]));
			Contract.Assert(new _SourceLocation(0, 1).Equals(greatgrandchild.Attributes.SourceLocation));
		}

	}
}
