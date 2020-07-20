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

        public static Task InvokeAsync<TComponent>(this IRenderedComponentBase<TComponent> componentBase, Func<Task> callback)
            where TComponent : IComponent
        {
            var source = new TaskCompletionSource<object?>();
            async void action()
            {
                try
                {
                    await callback();
                }
                catch (Exception e)
                {
                    source!.SetException(e);
                    return;
                }
                source!.SetResult(null);
            }
            componentBase.InvokeAsync(action);
            return source.Task;
        }
    }
}