using JBSnorro.Testing;
using JBSnorro.Threading;
using System.Linq;
using XunitAsyncTestSyncContext = Xunit.Sdk.AsyncTestSyncContext;

if (args.Contains("--async"))
{
    var syncContext = new XunitAsyncTestSyncContext(null); // new Nito.AsyncEx.AsyncContext().SynchronizationContext
    using (syncContext.AsTemporarySynchronizationContext())
    {
        await TestExtensions.DefaultMainTestProjectImplementation(args);
    }
    var exception = await syncContext.WaitForCompletionAsync();

    object? _ = exception switch
    {
        AggregateException a when a.InnerExceptions.Count == 1 => throw a.InnerExceptions[0],
        Exception e => throw e,
        null => null,
    };
}
else
{
    await TestExtensions.DefaultMainTestProjectImplementation(args);
}

