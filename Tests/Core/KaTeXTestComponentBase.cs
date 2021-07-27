#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using BlaTeX.Components;
using Bunit.Rendering;
using JBSnorro;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;

namespace BlaTeX.Tests
{
	public abstract class KaTeXTestComponentBase : IComponent
	{
		[Parameter]
		public RenderFragment<KaTeX> KaTeX { get; set; } = default!;

		/// <summary>
		/// Renders the component to the supplied <see cref="RenderTreeBuilder"/>.
		/// </summary>
		/// <param name="builder">The builder to use for rendering.</param>
		protected virtual void BuildRenderTree(RenderTreeBuilder builder)
		{
			Console.WriteLine("Building TestBase");
		}

		/// <summary>
		/// Entry point for the test.
		/// </summary>
		[Fact]
		public virtual async Task RazorTests()
		{
			// this simulates the work done by Xunit.Sdk.RazorTestDiscoverer (in Bunit.Xunit.dll)
			// because my testing library doesn't support that, but I want debugging!
			using var razorRenderer = new TestComponentRenderer();
			var tests = razorRenderer.GetRazorTestsFromComponent(this.GetType()).ToReadOnlyList();
			if (tests.Count == 1)
			{
				// this helps debugging tremendously because the foreach just swallows exceptions
				// I mean it doesn't catch them, but the IDE isn't showing them
				tests[0].RunTestAsync().Wait();
			}
			else
			{
				int i = 1;
				foreach (var test in tests)
				{
					Console.WriteLine($"Executing test {i}/{tests.Count}: '{test.Description}'");
					await test.RunTestAsync().ConfigureAwait(false);
					i++;
				}
			}

		}

		void IComponent.Attach(RenderHandle renderHandle) => renderHandle.Render(BuildRenderTree);

		/// <inheritdoc/>
		Task IComponent.SetParametersAsync(ParameterView parameters)
		{
			var all = parameters.ToDictionary();
			if (all.Count == 0)
				// in initialization
				return Task.CompletedTask;

			if (all.Count != 1 || all.All(kvp => kvp.Key == nameof(KaTeX)))
			{
				throw new ArgumentException($"Expected only one element, {nameof(KaTeX)}");
			}
			object element = all[nameof(KaTeX)];
			if (element is RenderFragment<KaTeX> fragment)
				this.KaTeX = fragment;
			else
				throw new ArgumentException($"Element {nameof(KaTeX)} must be of type {nameof(KaTeX)}");

			return Task.CompletedTask;
		}
	}
}
