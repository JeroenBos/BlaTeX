using JBSnorro.Testing;
using JBSnorro.Threading;
using System;
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
                    TestExtensions.DefaultMainTestProjectImplementation(args);
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
                TestExtensions.DefaultMainTestProjectImplementation(args);
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
    }
}