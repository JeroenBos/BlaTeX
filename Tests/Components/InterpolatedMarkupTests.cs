using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunit;
using Bunit.Extensions;
using JBSnorro;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BlaTeX.Tests.Mocks;

namespace BlaTeX.Tests.Components
{
	public class InterpolatedMarkupTests
	{
		protected static TestContext CreateTestContext(Option<string> renderToStringReturnValue = default)
		{
			var ctx = new TestContext();
			ctx.Services.AddDefaultTestContextServices(ctx, new BunitJSInterop());
			ctx.Services.Add<IKaTeXRuntime>(new MockKaTeXRuntime().With(renderToStringReturnValue));

			return ctx;
		}
		static readonly Func<string, IEnumerable<Range>> dummySelector = s => throw new NotImplementedException();

		[Fact]
		public async Task CreatingWithoutArgumentsThrows()
		{
			var cut = new InterpolatedMarkup();

			await Assert.ThrowsAsync<ArgumentException>(nameof(InterpolatedMarkup.Markup), () => cut.SetParametersAsync(ParameterView.Empty));
			await Assert.ThrowsAsync<ArgumentException>(() => cut.SetParametersAsync(new { Markup = "" }));
			await Assert.ThrowsAsync<ArgumentException>(nameof(InterpolatedMarkup.Substitute), () => cut.SetParametersAsync(new { Markup = "", Selector = dummySelector }));
		}
		[Fact]
		public async Task CreatingWithNullArgumentsThrows()
		{
			var cut = new InterpolatedMarkup();

			await Assert.ThrowsAsync<ArgumentNullException>(nameof(InterpolatedMarkup.Markup), () => cut.SetParametersAsync(new { Markup = (string)null }));
			await Assert.ThrowsAsync<ArgumentNullException>(nameof(InterpolatedMarkup.Selector), () => cut.SetParametersAsync(new { Markup = "", Selector = (Func<string, IEnumerable<Range>>)null }));
			await Assert.ThrowsAsync<ArgumentNullException>(nameof(InterpolatedMarkup.Substitute), () => cut.SetParametersAsync(new { Markup = "", Selector = dummySelector, Substitute = (object)null }));
		}
		[Fact]
		public async Task CreatingWithWrongTypeThrows()
		{
			var cut = new InterpolatedMarkup();
			await Assert.ThrowsAsync<InvalidCastException>(() => cut.SetParametersAsync(new { Markup = "", Selector = dummySelector, Substitute = new object() }));
		}
	}
}
