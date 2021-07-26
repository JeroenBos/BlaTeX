using JBSnorro.Diagnostics;
using JBSnorro.Extensions;
using JBSnorro.Testing;
using JBSnorro.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XunitAsyncTestSyncContext = Xunit.Sdk.AsyncTestSyncContext;

namespace BlaTeX.Tests
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			if (args.Contains("--async"))
			{
				var syncContext = new XunitAsyncTestSyncContext(null); // new Nito.AsyncEx.AsyncContext().SynchronizationContext
				using (syncContext.AsTemporarySynchronizationContext())
				{
					await TestExtensions.DefaultMainTestProjectImplementation(args);
				}
				var exception = await syncContext.WaitForCompletionAsync();

				var _ = exception switch
				{
					AggregateException a when a.InnerExceptions.Count == 1 => throw a.InnerExceptions[0],
					Exception e => throw e,
					null => (object)null,
				};
			}
			else
			{
				// HelperToFindTestTypes(args);
				await TestExtensions.DefaultMainTestProjectImplementation(args);
			}
		}
		public static string RootFolder
		{
			get
			{
				var dir = new DirectoryInfo(Environment.CurrentDirectory);
				while (dir.Name != "BlaTeX")
				{
					dir = dir.Parent;
					if (dir == null)
						throw new InvalidOperationException("Cannot find project root folder");
				}
				return dir.FullName;
			}
		}

		internal static void HelperToFindTestTypes(IReadOnlyList<string> args)
		{
			Contract.Requires(args.Count != 0);

			string filter = args[0];
			var types = typeof(Program).Assembly
									   .GetTypes()
									   .Where(type => type.Namespace.Contains("BlaTeX"))
									   .Where(type => !type.Name.StartsWith("<"))
									   .Where(type => type.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase))
									   .ToList();

			if (types.Count == 0)
				Console.WriteLine($"No types in this assembly have speakable names matching '{filter}'.");
			foreach (Type type in types.OrderBy(t => t.FullName.Substring(t.Namespace.Length + ".".Length)))
			{
				string name = type.FullName.Substring(type.Namespace.Length + ".".Length);
				int testCount = TestExtensions.GetTestMethods(type).Count();
				Console.WriteLine($"{name} has {testCount} tests");
			}
		}
	}
}
