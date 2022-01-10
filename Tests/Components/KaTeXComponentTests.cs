using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlaTeX.JSInterop.KaTeX;
using BlaTeX.JSInterop.KaTeX.Syntax;
using Bunit;
using Bunit.Extensions;
using JBSnorro;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlaTeX.Tests.Mocks;

namespace BlaTeX.Tests.Components
{
	using KaTeX = BlaTeX.Components.KaTeX;
	public class KaTeXComponentTests
	{
		protected static TestContext CreateTestContext(Option<string> renderToStringReturnValue = default)
		{
			var ctx = new TestContext();
			ctx.Services.AddDefaultTestContextServices(ctx, new BunitJSInterop());
			ctx.Services.Add<IKaTeXRuntime>(new MockKaTeXRuntime().With(renderToStringReturnValue));

			return ctx;
		}


		[@Fact]
		public async Task CannotSetNullMathAndAST()
		{
			var cut = new KaTeX();

			await Assert.ThrowsAsync<ArgumentException>(() => cut.SetParametersAsync(ParameterView.Empty));
		}
		[@Fact]
		public void CannotSetNullMathAndASTAfterTheyveBeenSet()
		{
			using var ctx = CreateTestContext();
			var cut = ctx.RenderComponent<KaTeX>(ComponentParameter.CreateParameter("Math", "a"));

			Assert.Throws<ArgumentException>(() => cut.SetParametersAndRerender(ComponentParameter.CreateParameter("Math", null)));
			Assert.Equal(1, cut.RenderCount);
		}
		[@Fact]
		public void CanSetMathAgain()
		{
			using var ctx = CreateTestContext();
			var cut = ctx.RenderComponent<KaTeX>(ComponentParameter.CreateParameter("Math", "a"));

			cut.SetParametersAndRerender(ComponentParameter.CreateParameter("Math", "b"));
			Assert.Equal(2, cut.RenderCount);
		}
		[@Fact]
		public void SettingsMarkupServiceToNullIsPossible()
		{
			using var ctx = CreateTestContext();
			var cut = ctx.RenderComponent<KaTeX>(ComponentParameter.CreateParameter("Math", "a"));

			Assert.Equal(1, cut.RenderCount);
			cut.SetParametersAndRerender(ComponentParameter.CreateParameter("ChildComponentMarkupService", null));
			Assert.Equal(2, cut.RenderCount);
		}

	}
}
