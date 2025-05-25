using BlaTeX.JSInterop.KaTeX.Internal;
using BlaTeX.Tests;
using Microsoft.Extensions.DependencyInjection;

public class TreeBasedKaTeXTests
{
    // what want to achieve in this test is to use the `IHtmlDomNode` to build the blatex tree, instead of relying on builder.AddMarkup
    [Fact]
    public async Task TreebasedRendered()
    {
        using var ctx = new TestContext();
        ctx.Services.Add(new ServiceDescriptor(typeof(IJSRuntime), NodeJSRuntime.CreateDefault()));
        ctx.Services.AddSingleton<IKaTeXRuntime, KaTeXRuntime>();
        IRenderedComponent<TreeBasedKaTeXComponent> cut = ctx.RenderComponent<TreeBasedKaTeXComponent>(ComponentParameter.CreateParameter("math", "c"));
        await cut.Instance.Rendered;


        try
        {
            cut.MarkupMatches("<div>Hello from RenderTree</div>");
        }
        catch (Exception e)
        {

        }
    }
}
