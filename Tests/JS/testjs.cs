using JBSnorro;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using static JBSnorro.Diagnostics.Contract;

namespace BlaTeX.Tests
{
    public class JSIntegrationTests
    {
        [Fact]
        public async Task JSInteropTestViaBlazor()
        {
            var imports = $"{Program.RootFolder}/wwwroot/js/blatex_wrapper.js".ToSingleton();
            var result = await new NodeJSRuntime(imports).InvokeAsync<string>("blatex_wrapper.default.renderToString", new[] { "c" }).AsTask();
            Assert(result == "<span class=\"katex\"><span class=\"katex-mathml\"><math><semantics><mrow><mi>c</mi></mrow><annotation encoding=\"application/x-tex\">c</annotation></semantics></math></span><span class=\"katex-html\" aria-hidden=\"true\"><span class=\"base\"><span class=\"strut\" style=\"height:0.43056em;vertical-align:0em;\"></span><span class=\"mord mathit\">c</span></span></span></span>");
        }
    }
}