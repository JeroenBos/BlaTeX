using JBSnorro.Testing;
using JBSnorro.Threading;
using System;
using System.Diagnostics;
using System.IO;
using NitoAsyncContext = Nito.AsyncEx.AsyncContext;

namespace BlaTeX.Tests
{
    public class Program
    {
        [DebuggerHidden]
        public static void Main(string[] args)
        {
            using (new NitoAsyncContext().SynchronizationContext.AsTemporarySynchronizationContext())
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