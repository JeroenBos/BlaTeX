using Microsoft.AspNetCore.Components.Rendering;

public class InterpolatedMarkup : ComponentBase
{
    /// <summary> Markup of which parts may be replaced by fragment renderers. </summary>
    [Parameter]
    public required string Markup { get; init; }

    /// <summary> Gets the ranges in <see cref="Markup"/> where fragments are to be rendered instead. </summary>
    [Parameter]
    public required Func<string, IEnumerable<Range>> Selector { get; init; }

    /// <summary> Given a substring of <see cref="Markup"/>, gets the fragment to render. </summary>
    [Parameter]
    public required Func<string, RenderFragment> Substitute { get; init; }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        parameters.AssertPresent(this.Markup, nameof(this.Markup));
        parameters.AssertPresent(this.Selector, nameof(this.Selector));
        parameters.AssertPresent(this.Substitute, nameof(this.Substitute));

        return base.SetParametersAsync(parameters);
    }
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        int seq = 0;
        Index prevEnd = 0;

        foreach (var range in Selector(Markup))
        {
            builder.AddMarkupContent(seq++, Markup[prevEnd..range.Start]);
            builder.AddMarkupContent(seq++, Substitute(Markup[range.Start..range.End]));
            prevEnd = range.End;
        }

        builder.AddMarkupContent(seq++, Markup[prevEnd..]);

        Console.WriteLine(builder.ToString());
    }
}
