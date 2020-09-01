using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Linq;
using JBSnorro.Extensions;

namespace BlaTeX.Tests
{
	public static class RendererSynchronizationContext
	{
		/// <summary> Gets the singleton instance of Blazor's renderer synchronization context. </summary>
		public static SynchronizationContext Instance { get; } = Create();
		public static SynchronizationContext Create()
		{
			const string AssemblyName = "Microsoft.AspNetCore.Components";
			const string RenderSynchronizationContextTypeName = "Microsoft.AspNetCore.Components.Rendering.RendererSynchronizationContext";

			var type = Assembly.Load(AssemblyName).GetType(RenderSynchronizationContextTypeName);
			var ctor = type.GetConstructors().Single();
			var context = (SynchronizationContext)ctor.Invoke(Array.Empty<object>());
			return context;
		}


		/// <summary> Sets the current synchronization context to Blazor's internal RenderSynchronizationContext. </summary>
		public static void SetContext(bool throwIfAlreadySet = false)
		{
			if (throwIfAlreadySet && SynchronizationContext.Current == Instance)
			{
				throw new InvalidOperationException("The rendering synchronization context has already been set");
			}

			SynchronizationContext.SetSynchronizationContext(Instance);
		}
	}
}