#nullable enable
using Xunit;
using BlaTeX.JSInterop;
using System.Threading.Tasks;
using JBSnorro.Diagnostics;
using BlaTeX.JSInterop.KaTeX;

namespace BlaTeX.Tests
{
    public class HtmlDomNodeTests
    {
        private readonly IKaTeX KaTeX = IKaTeX.Create(NodeJSRuntime.CreateDefault());

        [Fact]
        public async Task FirstHtmlDomNodeTests()
        {
            var domNode = await KaTeX.RenderToDom("c") as DomSpan;
            Contract.Assert(domNode != null);
            Contract.Assert(domNode.Children != null);
            Contract.Assert(domNode.Children.Count == 2);


        }

    }
}