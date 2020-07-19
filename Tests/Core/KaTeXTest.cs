#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using BlaTeX.Pages;
using Bunit;
using Bunit.Diffing;
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

namespace BlaTeX.Tests
{
    /// <summary>
    /// A component used to create KaTeX snapshot tests.
    /// Snapshot tests takes the math string and options as inputs, and an Expected section.
    /// It then compares the result of letting the katex library render the inputs, using semantic HTML comparison.
    /// </summary>
    public class KaTeXTest : RazorTestBase
    {
        /// <inheritdoc/>
        public override string? DisplayName => this.GetType().Name;
        [Parameter]
        public string? Math { get; set; } = default!;
        /// <summary> If specified, dictates whether the interactive version of the component is tested or not.
        /// If not specified, it depends on whether <see cref="Action"> is provided.  </summary>
        [Parameter]
        public bool? Interactive { get; set; }
        [Parameter]
        public RenderFragment Expected { get; set; } = default!;
        /// <summary> An action to be executed after the first render but before the snapshot comparison. </summary>
        [Parameter]
        public EventCallback<IRenderedComponent<KaTeX>> Action { get; set; }

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
            var parameters = new ComponentParameter[] { (nameof(KaTeX.Math), this.Math) };
            bool interactive = this.Interactive ?? this.Action.HasDelegate;
            if (interactive)
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
                await this.Action.InvokeAsync(renderedCut);

            var katexHtml = Htmlizer.GetHtml(Renderer, id);
            var expectedRenderId = Renderer.RenderFragment(this.Expected);
            var expectedHtml = Htmlizer.GetHtml(Renderer, expectedRenderId);

            HtmlEqualityComparer.AssertEqual(expectedHtml, katexHtml);
        }

        private async Task<IRenderedComponent<KaTeX>> WaitForKatexToHaveRendered(KaTeX cut, int cutId, TimeSpan? timeout = default)
        {
            var icut = cut.ToIRenderedComponent(cutId, this.Services);
            using var waiter = new WaitForStateHelper(icut, () => cut.markup != null, timeout);
            await waiter.WaitTask; // don't just return the task because then the waiter is disposed of too early
            return icut!;
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
            if (d.TryGetValue("math", out object? math))
                this.Math = (string)math!;
            if (d.TryGetValue("ChildContent", out object? expected))
                this.Expected = (RenderFragment)expected;
            if (d.TryGetValue("Action", out object? action))
                this.Action = (EventCallback<IRenderedComponent<KaTeX>>)action;

            var unknown = d.Keys.Where(key => key != "math" && key != "ChildContent" && key != "Action").ToList();
            if (unknown.Count != 0)
                throw new ArgumentException($"Unsupported parameter received", unknown[0]);

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

