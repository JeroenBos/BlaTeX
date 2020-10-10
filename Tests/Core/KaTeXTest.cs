#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using BlaTeX.Pages;
using Bunit;
using Bunit.Extensions;
using Bunit.Extensions.WaitForHelpers;
using Bunit.RazorTesting;
using Bunit.Rendering;
using JBSnorro;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using BlaTeX.JSInterop;
using BlaTeX.JSInterop.KaTeX;
using System.Threading;

namespace BlaTeX.Tests
{
	/// <summary>
	/// A component used to create KaTeX snapshot tests.
	/// Snapshot tests takes the math string and options as inputs, and an Expected section.
	/// It then compares the result of letting the katex library render the inputs, using semantic HTML comparison.
	/// </summary>
	public class KaTeXTest : RazorTestBase
	{
		// test-related parameters: 

		/// <summary> If specified, dictates whether the interactive version of the component is tested or not.
		/// If not specified, it depends on whether <see cref="Action"> is provided.  </summary>
		[Parameter]
		public bool? Interactive { get; set; }
		[Parameter]
		public RenderFragment? TestInput { get; set; }
		[Parameter]
		public RenderFragment Expected { get; set; } = default!;
		/// <summary> An action to be executed after the first render but before the snapshot comparison. </summary>
		[Parameter]
		public EventCallback<IRenderedComponent<KaTeX>> Action { get; set; }
		public override string Description => base.Description ?? Math ?? "No description";


		// parameter passed on to component under test:

		[Parameter]
		public string Math { get; set; } = default!;
		[Parameter]
		public IChildComponentMarkupService? ChildComponentMarkupService { get; set; }



		/// <inheritdoc/>
		public override string? DisplayName => this.GetType().Name;

		/// <inheritdoc/>
		protected override async Task Run()
		{
			Validate();

			string blatexJSPath = Program.RootFolder + "/wwwroot/js/blatex_wrapper.js";
			if (!File.Exists(blatexJSPath)) throw new FileNotFoundException("blatex.js not found. You probably need to build it, see readme. ");

			Services.AddDefaultTestContextServices();
			Services.Add(new ServiceDescriptor(typeof(IJSRuntime), new NodeJSRuntime(new[] { blatexJSPath })));
			Services.Add(new ServiceDescriptor(typeof(IKaTeX), typeof(_KaTeX), ServiceLifetime.Singleton));

			int id;
			KaTeX cut;
			var parameters = new ComponentParameter[]
			{
				(nameof(KaTeX.Math), this.Math),
				(nameof(KaTeX.ChildComponentMarkupService), this.ChildComponentMarkupService),
			};
			if (this.Interactive ?? false)
			{
				(id, cut) = this.Renderer.RenderComponent<InteractiveKaTeX>(parameters);
			}
			else
			{
				(id, cut) = this.Renderer.RenderComponent<KaTeX>(parameters);
			}

			if (cut is null)
				throw new InvalidOperationException("The KaTeX component did not render successfully");

			var renderedCut = await WaitForKatexToHaveRendered(cut, id);
			if (this.Action.HasDelegate)
			{
				Console.WriteLine("this.Action.HasDelegate");
				Console.Out.Flush();
				await this.Action.InvokeAsync(renderedCut);
				Console.WriteLine("DEBUG going to WaitForKatexToHaveRendered");
				Console.Out.Flush();
				await WaitForKatexToHaveRendered(cut, id);
				Console.WriteLine("DEBUG waited");
				Console.Out.Flush();
				
			}

			var katexHtml = Htmlizer.GetHtml(Renderer, id);
			var expectedRenderId = Renderer.RenderFragment(this.Expected);
			var expectedHtml = Htmlizer.GetHtml(Renderer, expectedRenderId);

			HtmlEqualityComparer.AssertEqual(expectedHtml, katexHtml);
		}
		private async Task<IRenderedComponent<KaTeX>> WaitForKatexToHaveRendered(KaTeX cut, int cutId, TimeSpan? timeout = default)
		{
			var icut = cut.ToIRenderedComponent(cutId, this.Services);
			Console.WriteLine("DEBUG creating waiter wait");
			Console.Out.Flush();
			using var waiter = new WaitForStateHelper(icut, predicate, TimeSpan.FromSeconds(3));
			Console.WriteLine("DEBUG going to wait");
			Console.Out.Flush();
			await waiter.WaitTask; // don't just return the task because then the waiter is disposed of too early
			Console.WriteLine("DEBUG waited");
			Console.Out.Flush();
			return icut!;

			bool predicate()
			{
				return cut.markup != null;
			}

		}

		/// <inheritdoc/>
		public override void Validate()
		{
			base.Validate();
			if (Math is null)
				throw new ArgumentException($"No {nameof(Math)} specified in the {nameof(KaTeXTest)} component.");
			if (Expected is null)
				throw new ArgumentException($"No child contents specified in the {nameof(KaTeXTest)} component.");
		}

		public override Task SetParametersAsync(ParameterView parameters)
		{
			var d = parameters.ToDictionary();
			foreach (var (key, value) in d)
			{
				switch (key)
				{
					case "math":
						this.Math = (string)value!;
						break;
					case "ChildContent":
						this.Expected = (RenderFragment)value;
						break;
					case "Action":
						this.Action = (EventCallback<IRenderedComponent<KaTeX>>)value;
						break;
					case "ChildComponentMarkupService":
						this.ChildComponentMarkupService = (IChildComponentMarkupService)value;
						break;
					default:
						throw new ArgumentException($"Unsupported parameter received: '{key}'", nameof(parameters));
				}

			}
			return base.SetParametersAsync(parameters);
		}
	}
	static class IRenderedComponentConstructorExtension
	{
		private static readonly Type RenderedComponentType = typeof(SnapshotTest).Assembly.GetType("Bunit.Rendering.RenderedComponent`1")!.MakeGenericType(typeof(KaTeX));
		private static readonly ConstructorInfo RenderedComponentTypeCtor = RenderedComponentType.GetConstructors()[0];

		/// <summary>
		/// Instantiates and performs a first render of a component of type <typeparamref name="KaTeX"/>.
		/// </summary>
		/// <typeparam name="KaTeX">Type of the component to render</typeparam>
		/// <returns>The rendered <typeparamref name="KaTeX"/></returns>
		public static IRenderedComponent<KaTeX> ToIRenderedComponent(this KaTeX cut, int cutId, TestServiceProvider services)
		{
			return (IRenderedComponent<KaTeX>)RenderedComponentTypeCtor.Invoke(new object[] { services, cutId, cut });
		}
	}
}

