using Bunit.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BlaTeX.Tests
{
    public static class RendererAwaiter
    {
        private static readonly MethodInfo ProcessAsynchronousWork; // has signature like Func<Task>
        private static readonly FieldInfo _pendingTasks; // of type List<Task>
        static RendererAwaiter()
        {
            ProcessAsynchronousWork = typeof(Renderer).GetMethod(nameof(ProcessAsynchronousWork), BindingFlags.NonPublic | BindingFlags.Instance)!;
            _pendingTasks = typeof(Renderer).GetField(nameof(_pendingTasks), BindingFlags.NonPublic | BindingFlags.Instance)!;
        }

        /// <summary> Waits for all pending tasks in the specified renderer. </summary>
        public static Task Wait(this Renderer renderer)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            if (_pendingTasks.GetValue(renderer) == null)
                return Task.CompletedTask;

            return (Task)ProcessAsynchronousWork.Invoke(renderer, Array.Empty<object>());
        }
        public static Task Wait(this ITestRenderer renderer) => Wait((Renderer)renderer);
    }
}