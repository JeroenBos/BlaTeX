#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using BlaTeX.Pages;
using Bunit.Diffing;
using Bunit.Extensions;
using Bunit.RazorTesting;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

using JBSnorro;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Bunit
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
        /// Called by the XUnit test runner. Finds all Fixture components
        /// in the file and runs their associated tests.
        /// </summary>
        [Fact]
        public virtual void RazorTests()
        {
            // this simulates the work done by Xunit.Sdk.RazorTestDiscoverer (in Bunit.Xunit.dll)
            // because my testing library doesn't support that, but I want debugging!
            using var razorRenderer = new TestComponentRenderer();
            var tests = razorRenderer.GetRazorTestsFromComponent(this.GetType()).ToReadOnlyList();
            foreach (var test in tests)
            {
                test.RunTest();
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

            // this.Math = parameters.GetValueOrDefault<string>(nameof(Math));
            // var all = parameters.ToDictionary();
            // this.Expected = all.Where(kvp => kvp.Value is RenderFragment)
            //                    .Select(kvp => kvp.Key)
            //                    .ToDictionary(valueSelector: key => (RenderFragment)all[key]);
            // return base.SetParametersAsync(parameters);
        }

        public virtual Task DoOperations(KaTeX cut) => Task.CompletedTask;
    }
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

        [Parameter]
        public RenderFragment Expected { get; set; } = default!;

        public KaTeXTest()
        {
        }

        /// <inheritdoc/>
        protected override Task Run()
        {
            Validate();

            Services.AddDefaultTestContextServices();

            var (id, cut) = this.Renderer.RenderComponent<KaTeX>(new ComponentParameter[] {
                (nameof(KaTeX.Math), this.Math)
            });

            if (cut is null)
                throw new InvalidOperationException("The KaTeX component did not render successfully");

            // await this.DoOperations(cut);

            var katexHtml = Htmlizer.GetHtml(Renderer, id);

            var expectedRenderId = Renderer.RenderFragment(this.Expected);
            var expectedHtml = Htmlizer.GetHtml(Renderer, expectedRenderId);

            VerifySnapshot(katexHtml, expectedHtml);
            return Task.CompletedTask;
        }


        private void VerifySnapshot(string inputHtml, string expectedHtml)
        {
            using var parser = new HtmlParser();
            var inputNodes = parser.Parse(inputHtml);
            var expectedNodes = parser.Parse(expectedHtml);

            var diffs = inputNodes.CompareTo(expectedNodes);

            if (diffs.Count > 0)
                throw new HtmlEqualException(diffs, expectedNodes, inputNodes, Description ?? "Snapshot test failed.");
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

            var unknown = d.Keys.Where(key => key != "math" && key != "ChildContent").ToList();
            if (unknown.Count != 0)
                throw new ArgumentException($"Unsupported parameter received", unknown[0]);

            return base.SetParametersAsync(parameters);
        }
    }
}

static class Htmlizer
{
    private static MethodInfo _mi;
    static Htmlizer()
    {
        var bunitAssembly = System.Reflection.Assembly.GetAssembly(typeof(Bunit.SnapshotTest));
        _mi = bunitAssembly!.GetType("Bunit.Htmlizer")!.GetMethod("GetHtml")!;
    }

    public static string GetHtml(ITestRenderer renderer, int componentId)
    {
        return (string)_mi.Invoke(null, new object[] { renderer, componentId })!;
    }
}