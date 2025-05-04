using BlaTeX.JSInterop.KaTeX.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace BlaTeX.Tests.Understanding;

public class AngleSharpTests
{
	[Fact]
	public async Task TryGetIgnoreDiffAngleSharpWorking()
	{
		using var ctx = new TestContext();
		ctx.Services.AddDefaultTestContextServices(ctx, new BunitJSInterop());
		ctx.Services.Add(new ServiceDescriptor(typeof(IJSRuntime), NodeJSRuntime.CreateDefault()));
		ctx.Services.Add(new ServiceDescriptor(typeof(IKaTeXRuntime), typeof(KaTeXRuntime), ServiceLifetime.Singleton));

		var cut = ctx.RenderComponent<KaTeX>(ComponentParameter.CreateParameter("Math", "c"));

		await KaTeXTest.WaitForKatexToHaveRendered(cut);

		const string expectedHtml = @"<span diff:ignore></span>";
		cut.MarkupMatches(expectedHtml);
	}
	[Fact]
	public async Task TryGetIgnoreDiffAngleSharpWorkingSimpler()
	{
		// Arrange
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
		// Arrange
		using var ctx = new TestContext();
		KaTeXTest.AddKaTeXTestDefaultServices(ctx);

		// Act
		var cut = ctx.RenderComponent<KaTeX>(ComponentParameter.CreateParameter("Math", "c"));
		await KaTeXTest.WaitForKatexToHaveRendered(cut);

		// Assert
		const string expectedHtml = @"<span diff:ignore></span>";
		HtmlEqualityComparer.AssertEqual(expected: expectedHtml, cut.Nodes, ctx.Services);
	}
	/// <summary>
	/// Currently in a test I have a nonbreaking whitespace present both in actual and expected, but the equality comparison chokes on it.
	/// Probably encoding.
	/// </summary>
	[Fact]
	public void Handle_NonbreakingWhitespace()
	{
		char nonbreakingWhitespace = '​';
		var difference = HtmlEqualityComparer.ComputeDifference($"<div>{nonbreakingWhitespace}</div>", "<div>&#8203;</div>");
		Assert.Empty(difference);
	}
}
