using System.Linq;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlaTeX.Tests;

public abstract class KaTeXTestComponentBase : IComponent
{
    [Parameter]
    public required RenderFragment<KaTeX> KaTeX { get; set; }

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
       
        int i = 1;
        foreach (var test in tests)
        {
            Console.WriteLine($"Executing test {i}/{tests.Count}: '{test.Description}'");
            await test.RunTestAsync().ConfigureAwait(true);
            i++;
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
        object? element = all[nameof(KaTeX)];
        if (element is RenderFragment<KaTeX> fragment)
            this.KaTeX = fragment;
        else
            throw new ArgumentException($"Element {nameof(KaTeX)} must be of type {nameof(KaTeX)}");

        return Task.CompletedTask;
    }
}
