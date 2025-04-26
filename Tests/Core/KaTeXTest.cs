using Bunit.RazorTesting;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace BlaTeX.Tests;

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

    public static void AddKaTeXTestDefaultServices(TestContextBase ctx)
    {
        Contract.Assert<BlatexNotFoundException>(File.Exists(NodeJSRuntime.DefaultJSPath.Value.Replace("\\\\", "\\")), "blatex.js not found. You probably need to build it, see readme. ");

        ctx.Services.AddDefaultTestContextServices(ctx, new BunitJSInterop());
        ctx.Services.Add(new ServiceDescriptor(typeof(IJSRuntime), NodeJSRuntime.CreateDefault()));
        ctx.Services.Add(new ServiceDescriptor(typeof(IKaTeXRuntime), typeof(KaTeXRuntime), ServiceLifetime.Singleton));
    }
    /// <inheritdoc/>
    protected override async Task RunAsync()
    {
        Validate();

        AddKaTeXTestDefaultServices(this);

        var parameters = new ComponentParameter[]
        {
                (nameof(KaTeX.Math), this.Math),
                (nameof(KaTeX.ChildComponentMarkupService), this.ChildComponentMarkupService),
        };
        IRenderedComponent<KaTeX> cut;
        if (this.Interactive ?? false)
        {
            cut = this.Renderer.RenderComponent<InteractiveKaTeX>(parameters);
        }
        else
        {
            cut = this.Renderer.RenderComponent<KaTeX>(parameters);
        }

        Contract.Assert(cut is not null, "The KaTeX component did not render successfully");

        await WaitForKatexToHaveRendered(cut);
        if (this.Action.HasDelegate)
        {
            await this.Action.InvokeAsync(cut);
            await WaitForKatexToHaveRendered(cut);
        }

        var katexHtml = cut.Markup;

        var expectedFragment = (IRenderedFragment)Renderer.RenderFragment(this.Expected);

        HtmlEqualException? exception = HtmlEqualityComparer.ComputeException(expectedFragment.Nodes, cut.Nodes);
        if (exception != null)
        {
            Console.WriteLine(exception.Message);
            System.Diagnostics.Debug.WriteLine(exception.Message);
            throw exception;
        }
    }
    internal static async Task WaitForKatexToHaveRendered(IRenderedComponent<KaTeX> cut)
    {
        // Use Task.Run to prevent program from exiting in case it's on the main thread
        await Task.Run(() => cut.WaitForState(predicate, WaitForStateTimeout));

        bool predicate() => cut.Instance.markup != null;
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
                    this.Expected = (RenderFragment)(value ?? throw new ArgumentNullException(nameof(Expected)));
                    break;
                case "Action":
                    this.Action = value is null ? default : (EventCallback<IRenderedComponent<KaTeX>>)value;
                    break;
                case "ChildComponentMarkupService":
                    this.ChildComponentMarkupService = (IChildComponentMarkupService?)value;
                    break;
                default:
                    throw new ArgumentException($"Unsupported parameter received: '{key}'", nameof(parameters));
            }

        }
        return base.SetParametersAsync(parameters);
    }
    public static TimeSpan WaitForStateTimeout
    {
        get
        {
            const string VAR_NAME = "WAIT_FOR_STATE_TIMEOUT_SEC";
            var value = Environment.GetEnvironmentVariable(VAR_NAME);
            if (value != null)
            {
                if (!int.TryParse(value, out int sec))
                    throw new ArgumentException($"Environment variable '{VAR_NAME}'='value' is not a value int");
                if (sec < -1)
                    throw new ArgumentNullException($"Environment variable '{VAR_NAME}' must be -1 or higher");

                if (sec == -1)
                    return System.Threading.Timeout.InfiniteTimeSpan;
                return TimeSpan.FromSeconds(sec);
            }

#if DEBUG
            // This value is hardcoded because I don't want to provide env args every time I type `dotnet test`
            return TimeSpan.FromMinutes(60); // TimeSpan.MaxValue doesn't work; 1 hour is long enough
#elif CI
                throw new ArgumentNullException($"No environment variable '{VAR_NAME}' provided in CI");
#else
                // host is assumed to be ASP.NET Core
                throw new ArgumentNullException($"No environment variable '{VAR_NAME}' provided");
#endif

        }
    }
}

