using System.Linq;
using Xunit;
using JBSnorro;
using Bunit;
using Bunit.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using KaTeX = BlaTeX.Pages.KaTeX; // prevents errors in VS
using BlaTeX.JSInterop.KaTeX;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BlaTeX.Tests.Understanding
{
	public class AngleSharp
	{
		[Fact]
		public async Task TryGetIgnoreDiffAngleSharpWorking()
		{
			using var ctx = new TestContext();
			ctx.Services.AddDefaultTestContextServices(ctx, new BunitJSInterop());
			ctx.Services.Add(new ServiceDescriptor(typeof(IJSRuntime), NodeJSRuntime.CreateDefault()));
			ctx.Services.Add(new ServiceDescriptor(typeof(IKaTeX), typeof(_KaTeX), ServiceLifetime.Singleton));

			var cut = ctx.RenderComponent<KaTeX>(ComponentParameter.CreateParameter("Math", "c"));

			await KaTeXTest.WaitForKatexToHaveRendered(cut);

			const string expectedHtml = @"<span diff:ignore></span>";
			cut.MarkupMatches(expectedHtml);
		}
		[Fact]
		public async Task TryGetIgnoreDiffAngleSharpWorkingSimpler()
		{
			// Arrage
			using var ctx = new TestContext();
			KaTeXTest.AddKaTeXTestDefaultServices(ctx);

			// Act
			var cut = ctx.RenderComponent<KaTeX>(ComponentParameter.CreateParameter("Math", "c"));
			await KaTeXTest.WaitForKatexToHaveRendered(cut);

			// Assert
			const string expectedHtml = @"<span diff:ignore></span>";
			cut.MarkupMatches(expectedHtml);
		}
		[Fact]
		public async Task TryGetIgnoreDiffAngleSharpWorkingThroughHowItsDoneInKaTeXTest()
		{
			// Arrage
			using var ctx = new TestContext();
			KaTeXTest.AddKaTeXTestDefaultServices(ctx);

			// Act
			var cut = ctx.RenderComponent<KaTeX>(ComponentParameter.CreateParameter("Math", "c"));
			await KaTeXTest.WaitForKatexToHaveRendered(cut);

			// Assert
			const string expectedHtml = @"<span diff:ignore></span>";
			HtmlEqualityComparer.AssertEqual(expected: expectedHtml, cut.Nodes, ctx.Services);
		}
	}
}
