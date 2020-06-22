using JBSnorro.Testing;
using System;
using System.Diagnostics;
using System.IO;

namespace BlaTeX.Tests
{
    class Program
    {
        [DebuggerHidden]
        public static void Main(string[] args) => TestExtensions.DefaultMainTestProjectImplementation(args);

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