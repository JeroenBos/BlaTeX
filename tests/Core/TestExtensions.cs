using AngleSharp.Dom;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BlaTeX.Tests;

internal static class BlaTeXTestExtensions
{
    /// <summary> 
    /// Clicks on the specified element, or on the first ancestor that listens to click events. 
    /// This is a workaround for the upstream issue https://github.com/egil/bUnit/issues/119.
    /// </summary>
    public static Task ClickWithFakeBubblingAsync(this IElement cut, MouseEventArgs? e = null)
    {
        Contract.Requires(cut != null);

        var result = cut;
        while (!result.HasAttribute("blazor:onclick"))
        {
            result = result.ParentElement;
            if (result?.ParentElement == null)
            {
                throw new Exception("No element or ancestor listens for click events, i.e. has the attribute @onclick");
            }
        }
        return result.ClickAsync(e ?? new MouseEventArgs());
    }
}


public static class AsyncMouseEventDispatchExtensions
{
    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseOver"/>
    public static Task MouseOverAsync(this IElement element, long detail = default, double screenX = default, double screenY = default, double clientX = default, double clientY = default, long button = default, long buttons = default, bool ctrlKey = default, bool shiftKey = default, bool altKey = default, bool metaKey = default, string? type = default)
        => MouseOverAsync(element, new MouseEventArgs { Detail = detail, ScreenX = screenX, ScreenY = screenY, ClientX = clientX, ClientY = clientY, Button = button, Buttons = buttons, CtrlKey = ctrlKey, ShiftKey = shiftKey, AltKey = altKey, MetaKey = metaKey, Type = type! });
    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseOver"/>
    public static Task MouseOverAsync(this IElement element, MouseEventArgs eventArgs) => element.TriggerEventAsync("onmouseover", eventArgs);

    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseOut"/>
    public static Task MouseOutAsync(this IElement element, long detail = default, double screenX = default, double screenY = default, double clientX = default, double clientY = default, long button = default, long buttons = default, bool ctrlKey = default, bool shiftKey = default, bool altKey = default, bool metaKey = default, string? type = default)
        => MouseOutAsync(element, new MouseEventArgs { Detail = detail, ScreenX = screenX, ScreenY = screenY, ClientX = clientX, ClientY = clientY, Button = button, Buttons = buttons, CtrlKey = ctrlKey, ShiftKey = shiftKey, AltKey = altKey, MetaKey = metaKey, Type = type! });
    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseOut"/>
    public static Task MouseOutAsync(this IElement element, MouseEventArgs eventArgs) => element.TriggerEventAsync("onmouseout", eventArgs);

    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseMove"/>
    public static Task MouseMoveAsync(this IElement element, long detail = default, double screenX = default, double screenY = default, double clientX = default, double clientY = default, long button = default, long buttons = default, bool ctrlKey = default, bool shiftKey = default, bool altKey = default, bool metaKey = default, string? type = default)
        => MouseMoveAsync(element, new MouseEventArgs { Detail = detail, ScreenX = screenX, ScreenY = screenY, ClientX = clientX, ClientY = clientY, Button = button, Buttons = buttons, CtrlKey = ctrlKey, ShiftKey = shiftKey, AltKey = altKey, MetaKey = metaKey, Type = type! });
    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseMove"/>
    public static Task MouseMoveAsync(this IElement element, MouseEventArgs eventArgs) => element.TriggerEventAsync("onmousemove", eventArgs);

    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseDown"/>
    public static Task MouseDownAsync(this IElement element, long detail = default, double screenX = default, double screenY = default, double clientX = default, double clientY = default, long button = default, long buttons = default, bool ctrlKey = default, bool shiftKey = default, bool altKey = default, bool metaKey = default, string? type = default)
        => MouseDownAsync(element, new MouseEventArgs { Detail = detail, ScreenX = screenX, ScreenY = screenY, ClientX = clientX, ClientY = clientY, Button = button, Buttons = buttons, CtrlKey = ctrlKey, ShiftKey = shiftKey, AltKey = altKey, MetaKey = metaKey, Type = type! });
    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseDown"/>
    public static Task MouseDownAsync(this IElement element, MouseEventArgs eventArgs) => element.TriggerEventAsync("onmousedown", eventArgs);

    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseUp"/>
    public static Task MouseUpAsync(this IElement element, long detail = default, double screenX = default, double screenY = default, double clientX = default, double clientY = default, long button = default, long buttons = default, bool ctrlKey = default, bool shiftKey = default, bool altKey = default, bool metaKey = default, string? type = default)
        => MouseUpAsync(element, new MouseEventArgs { Detail = detail, ScreenX = screenX, ScreenY = screenY, ClientX = clientX, ClientY = clientY, Button = button, Buttons = buttons, CtrlKey = ctrlKey, ShiftKey = shiftKey, AltKey = altKey, MetaKey = metaKey, Type = type! });
    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseUp"/>
    public static Task MouseUpAsync(this IElement element, MouseEventArgs eventArgs) => element.TriggerEventAsync("onmouseup", eventArgs);

    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseClick"/>
    public static Task ClickAsync(this IElement element, long detail = default, double screenX = default, double screenY = default, double clientX = default, double clientY = default, long button = default, long buttons = default, bool ctrlKey = default, bool shiftKey = default, bool altKey = default, bool metaKey = default, string? type = default)
        => ClickAsync(element, new MouseEventArgs { Detail = detail, ScreenX = screenX, ScreenY = screenY, ClientX = clientX, ClientY = clientY, Button = button, Buttons = buttons, CtrlKey = ctrlKey, ShiftKey = shiftKey, AltKey = altKey, MetaKey = metaKey, Type = type! });
    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseClick"/>
    public static Task ClickAsync(this IElement element, MouseEventArgs eventArgs) => element.TriggerEventAsync("onclick", eventArgs);

    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseDoubleClick"/>
    public static Task DoubleClickAsync(this IElement element, long detail = default, double screenX = default, double screenY = default, double clientX = default, double clientY = default, long button = default, long buttons = default, bool ctrlKey = default, bool shiftKey = default, bool altKey = default, bool metaKey = default, string? type = default)
        => DoubleClickAsync(element, new MouseEventArgs { Detail = detail, ScreenX = screenX, ScreenY = screenY, ClientX = clientX, ClientY = clientY, Button = button, Buttons = buttons, CtrlKey = ctrlKey, ShiftKey = shiftKey, AltKey = altKey, MetaKey = metaKey, Type = type! });
    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseDoubleClick"/>
    public static Task DoubleClickAsync(this IElement element, MouseEventArgs eventArgs) => element.TriggerEventAsync("ondblclick", eventArgs);

    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseDoubleWheel"/>
    public static Task WheelAsync(this IElement element, long detail = default, double screenX = default, double screenY = default, double clientX = default, double clientY = default, long button = default, long buttons = default, bool ctrlKey = default, bool shiftKey = default, bool altKey = default, bool metaKey = default, string? type = default, double deltaX = default, double deltaY = default, double deltaZ = default, long deltaMode = default)
        => WheelAsync(element, new WheelEventArgs { Detail = detail, ScreenX = screenX, ScreenY = screenY, ClientX = clientX, ClientY = clientY, Button = button, Buttons = buttons, CtrlKey = ctrlKey, ShiftKey = shiftKey, AltKey = altKey, MetaKey = metaKey, Type = type!, DeltaX = deltaX, DeltaY = deltaY, DeltaZ = deltaZ, DeltaMode = deltaMode });
    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseDoubleWheel"/>
    public static Task WheelAsync(this IElement element, WheelEventArgs eventArgs) => element.TriggerEventAsync("onwheel", eventArgs);

    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseMouseWheel"/>
    public static Task MouseWheelAsync(this IElement element, long detail = default, double screenX = default, double screenY = default, double clientX = default, double clientY = default, long button = default, long buttons = default, bool ctrlKey = default, bool shiftKey = default, bool altKey = default, bool metaKey = default, string? type = default, double deltaX = default, double deltaY = default, double deltaZ = default, long deltaMode = default)
        => MouseWheelAsync(element, new WheelEventArgs { Detail = detail, ScreenX = screenX, ScreenY = screenY, ClientX = clientX, ClientY = clientY, Button = button, Buttons = buttons, CtrlKey = ctrlKey, ShiftKey = shiftKey, AltKey = altKey, MetaKey = metaKey, Type = type!, DeltaX = deltaX, DeltaY = deltaY, DeltaZ = deltaZ, DeltaMode = deltaMode });
    /// <inheritdoc cref="MouseEventDispatchExtensions.MouseMouseWheel"/>
    public static Task MouseWheelAsync(this IElement element, WheelEventArgs eventArgs) => element.TriggerEventAsync("onmousewheel", eventArgs);

    /// <inheritdoc cref="MouseEventDispatchExtensions.ContextMenu"/>
    public static Task ContextMenuAsync(this IElement element, long detail = default, double screenX = default, double screenY = default, double clientX = default, double clientY = default, long button = default, long buttons = default, bool ctrlKey = default, bool shiftKey = default, bool altKey = default, bool metaKey = default, string? type = default)
        => ContextMenuAsync(element, new MouseEventArgs { Detail = detail, ScreenX = screenX, ScreenY = screenY, ClientX = clientX, ClientY = clientY, Button = button, Buttons = buttons, CtrlKey = ctrlKey, ShiftKey = shiftKey, AltKey = altKey, MetaKey = metaKey, Type = type! });
    /// <inheritdoc cref="MouseEventDispatchExtensions.ContextMenu"/>
    public static Task ContextMenuAsync(this IElement element, MouseEventArgs eventArgs) => element.TriggerEventAsync("oncontextmenu", eventArgs);
}
public static class MyRenderedComponentRenderExtensions
{
    public static void Add<T>(this TestServiceProvider services, T value) where T : notnull
    {
        services.Add(new ServiceDescriptor(typeof(T), value));
    }

    /// <summary>
    /// Render the component under test again with the provided <paramref name="parameters"/>.
    /// The difference with <see cref="RenderedComponentRenderExtensions.SetParametersAndRender"/> is that this will not swallow exceptions.
    /// </summary>
    /// <param name="renderedComponent">The rendered component to re-render with new parameters.</param>
    /// <param name="parameters">Parameters to pass to the component upon rendered.</param>
    /// <typeparam name="TComponent">The type of the component.</typeparam>
    public static void SetParametersAndRerender<TComponent>(this IRenderedComponentBase<TComponent> renderedComponent, params ComponentParameter[] parameters)
        where TComponent : IComponent
    {
        if (renderedComponent is null)
            throw new ArgumentNullException(nameof(renderedComponent));
        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters));

        SetParametersAndRerender(renderedComponent, ToParameterView(parameters));
    }
    /// <summary>
    /// Render the component under test again with the provided <paramref name="parameters"/>.
    /// The difference with <see cref="RenderedComponentRenderExtensions.SetParametersAndRender"/> is that this will not swallow exceptions.
    /// <param name="renderedComponent">The rendered component to re-render with new parameters.</param>
    /// <param name="parameters">Parameters to pass to the component upon rendered.</param>
    /// </summary>
    public static void SetParametersAndRerender<TComponent>(this IRenderedComponentBase<TComponent> renderedComponent, ParameterView parameters)
        where TComponent : IComponent
    {
        if (renderedComponent is null)
            throw new ArgumentNullException(nameof(renderedComponent));

        var result = renderedComponent.InvokeAsync(() => renderedComponent.Instance.SetParametersAsync(parameters));

        if (!result.IsCompleted)
        {
            result.GetAwaiter().GetResult();
        }
        else if (result.Status == TaskStatus.Faulted)
        {
            throw result.Exception!.InnerException!;
        }
    }

    private static readonly Func<IReadOnlyCollection<ComponentParameter>, ParameterView> toParameterView = fetchToParameterView();
    private static Func<IReadOnlyCollection<ComponentParameter>, ParameterView> fetchToParameterView()
    {
        var mi = typeof(RenderedComponentRenderExtensions).GetMethod(nameof(ToParameterView), BindingFlags.Static | BindingFlags.NonPublic)!;
        return mi.CreateDelegate<Func<IReadOnlyCollection<ComponentParameter>, ParameterView>>();
    }
    /// <summary> Converts the component parameters to a parameter view, with contract assertions. </summary>
    public static ParameterView ToParameterView(IReadOnlyCollection<ComponentParameter> parameters) => toParameterView(parameters);
}
