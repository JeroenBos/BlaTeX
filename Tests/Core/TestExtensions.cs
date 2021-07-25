#nullable enable
using AngleSharp.Dom;
using BlaTeX.JSInterop;
using BlaTeX.JSInterop.KaTeX;
using Bunit;
using JBSnorro.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BlaTeX.Tests
{
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
				if (result.ParentElement == null)
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
}
