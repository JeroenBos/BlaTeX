using System.Diagnostics;
using Microsoft.AspNetCore.Components.Rendering;

class TreeBasedKaTeXComponent : IComponent
{
    private readonly IKaTeXRuntime katex;
    private string? math;
    private IHtmlDomNode? domNode;
    private RenderHandle renderer;
    private bool isRendering;

    private readonly TaskCompletionSource rendering = new();
    public Task Rendered => rendering.Task;

    public TreeBasedKaTeXComponent(IKaTeXRuntime katex)
    {
        this.katex = katex ?? throw new ArgumentNullException(nameof(katex));
    }
    public async Task SetParametersAsync(ParameterView parameters)
    {
        if (parameters.TryGetValue(nameof(math), out this.math))
        {
            ArgumentNullException.ThrowIfNull(this.math, nameof(math));
            isRendering = true;
            try
            {
                this.domNode = await katex.RenderToDom(this.math);
                this.renderer.Render(this.BuildRenderTree);
            }
            finally
            {
                isRendering = false;
            }
            Contract.Ensures(this.domNode is not null);
        }
        else
        {
            throw new ArgumentException($"Required parameter '{nameof(math)}' not set");
        }
    }
    protected void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (this.domNode is null)
        {
            if (this.isRendering)
                throw new NotImplementedException("We're still creating the KateX dom, but BuildRenderTree is already being computed");
            else
                throw new UnreachableException("Setting math (which is mandatory) should have set the domNode as well");
        }
        try
        {
            Add(builder, this.domNode, seq: 0);
            builder.AddMarkupContent(1, "<div>at least this</div>");
        }
        catch (Exception e)
        {
            this.rendering.SetException(e);
            return;
        }
        this.rendering.SetResult();
    }
    private static void Add(RenderTreeBuilder builder, IHtmlDomNode element, int seq)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(element);
        ArgumentException.ThrowIfNullOrEmpty(element.Tag);

        builder.OpenElement(seq, element.Tag);
        if (element.Classes?.Count > 1)
        {
            builder.AddAttribute(seq + 1, "class", element.Classes.Join(" "));
        }
        if (element.Style != null)
        {
            builder.AddAttribute(seq + 2, "style", "");
        }
        if (element.Attributes.Count != 0)
        {
            builder.AddMultipleAttributes(seq + 3, element.Attributes!);
        }

        foreach (IHtmlDomNode child in element.Children)
        {
            if (string.IsNullOrEmpty(child.Text))
            {
                // Contract.Assert(child.Attributes.Count == 0);
                // Contract.Assert(child.Children.Count == 0);
                builder.AddContent(seq + 3, child.Text);
            }
            else
            {
                Add(builder, child, seq + 1);
            }
        }

        builder.CloseElement();
    }

    public void Attach(RenderHandle renderHandle)
    {
        if (renderer.IsInitialized)
            throw new InvalidOperationException("Already attacked");
        renderer = renderHandle;
    }

}

