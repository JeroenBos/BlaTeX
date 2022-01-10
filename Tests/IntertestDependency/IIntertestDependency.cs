#nullable enable
using Xunit;
using JBSnorro.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using JBSnorro.Extensions;
using JBSnorro.Testing;
using System.Net.WebSockets;
using TypeExtensions = ToBeJBSnorro.Extensions.TypeExtensions;

namespace JBSnorro.Tests.IntertestDependency
{
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using ToBeJBSnorro;

    public interface ITestIdentifier : IEquatable<ITestIdentifier>
    {
    }
    class TestIdentifier : ITestIdentifier
    {
        public string FullName { get; init; } = default!;
        public bool IsType { get; init; }
        public string TypeName => IsType ? FullName : FullName.SubstringUntilLast(".");
        public string TestName => IsType ? throw new InvalidTestConfigurationException() : FullName.SubstringAfterLast(".");

        public static TestIdentifier FromString(string identifier, Type callerType)
        {
            bool isQualifiedIdentifier = identifier.Contains('.');
            if (isQualifiedIdentifier)
            {
                var type = callerType.Assembly.GetType(identifier);
                if (type != null)
                {
                    return From(type);
                }

                string typeName = identifier.SubstringUntilLast(".");
                type = callerType.Assembly.GetType(typeName);
                if (type != null)
                {
                    return From(type, methodName: identifier.SubstringAfterLast("."));
                }
                throw new InvalidTestConfigurationException($"The identifier '{identifier}' could not be located");
            }
            else
            {
                var mi = callerType.GetMethod(identifier, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi != null)
                {
                    return From(mi);
                }
                throw new InvalidTestConfigurationException($"Could not resolve test '{identifier}' from type '{callerType.FullName}'");
            }
        }
        public static TestIdentifier From(MethodInfo mi)
        {
            string fullName = mi.DeclaringType == null
                            ? mi.Name
                            : mi.DeclaringType.FullName + "." + mi.Name;
            return new TestIdentifier { FullName = fullName, IsType = false };
        }
        public static TestIdentifier From(Type type)
        {
            string fullName = type.FullName!;
            return new TestIdentifier { FullName = fullName, IsType = true };
        }
        public static TestIdentifier From(Type type, string methodName)
        {
            var mi = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            if (mi == null)
            {
                throw new InvalidTestConfigurationException($"No method with name '{methodName}' exists in type '{type.FullName}'");
            }
            return From(mi);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TestIdentifier);
        }
        public virtual bool Equals(ITestIdentifier? other)
        {
            return Equals(other as TestIdentifier);
        }
        public virtual bool Equals(TestIdentifier? other)
        {
            return other != null && other.FullName == this.FullName;
        }
        public override int GetHashCode()
        {
            return FullName.GetHashCode();
        }
        public override string ToString()
        {
            return FullName;
        }
    }
    public static class IntertestExtensions
    {
        /// <param name="this"> The this parameter is useful two-fold: we can then infer the calling type, and second, 
        /// for convenience: `await this.DependsOn(SomeTest)` reads better than `await IntertestExtensions.DependsOn(SomeTest)`. </param>
        public static Task DependsOn(this object @this, string name)
        {
            ITestIdentifier identifier = TestIdentifier.FromString(name, @this.GetType());
            var dependencyTracker = IIntertestDependencyTracker.GetSingleton();
            return dependencyTracker.DependsOn(new[] { identifier });
        }
        //public static ITestIdentifier DependsOn(this object @this, string fullname, string fullname2, [CallerMemberName] string? memberName = null)
        //{
        //    string s = nameof(IIntertestDependency.DependsOn);
        //}
        //internal interface IIntertestDependency // use these methods instead
        //{
        //    Task DependsOn(params Type[] t);
        //    /// <summary>
        //    /// Asserts that the caller's dependencies have not failed yet; otherwise an Inconclusive test result is emitted.
        //    /// </summary>
        //    Task DependsOn(params Delegate[] o);
        //    Task DependsOn(params string[] t);
        //    Task DependsOn(params object[] t);
        //}
    }
    interface ITestRun
    {
        bool CompletedUnsuccessfully { get; }
        bool CompletedSuccessfully { get; }
        bool Pending { get; }
        Task<Task> TestTask { get; }
    }

    public interface IIntertestDependencyTracker
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
        /// <summary>
        /// Allows to configure how which IIntertestDependencyTracker is used for all tests.
        /// </summary>
        public static Func<IIntertestDependencyTracker> GetSingleton = () => IntertestDependencyTracker.singleton;
#pragma warning restore CA2211 // Non-constant fields should not be visible

        public static IIntertestDependencyTracker Singleton => GetSingleton();
        Task DependsOn(ITestIdentifier[] testsIdentifiers);
    }
    internal class IntertestDependencyTracker : IIntertestDependencyTracker
    {
        public static readonly IntertestDependencyTracker singleton = new();
        private ImmutableDictionary<ITestIdentifier, ITestRun> testResults;
        public IImmutableDictionary<ITestIdentifier, ITestRun> TestResults => testResults;

        private IntertestDependencyTracker() { this.testResults = ImmutableDictionary<ITestIdentifier, ITestRun>.Empty; }

        public async Task DependsOn(params TestIdentifier[] testsIdentifiers)
        {
            Contract.Requires(testsIdentifiers != null);
            Contract.Requires(Contract.ForAll(testsIdentifiers, _ => _ != null));
            Contract.Requires(Enumerable.Distinct(testsIdentifiers, testResults.KeyComparer).Count() == testsIdentifiers.Length);

            // could be parallelized, but should respect the test runner's parallelization
            // make sure that any InconclusiveTestException is propagated up here. All other exceptions should probably be converted?
            foreach (var test in testsIdentifiers)
            {
                try
                {
                    await this.DependsOn(test);
                }
                catch (SkipException)
                {
                    throw;
                }
                catch (InvalidTestConfigurationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new SkipException("Skipping because a dependency test failed", ex);
                }
            }
        }
        private async Task DependsOn(TestIdentifier testIdentifier)
        {
            var created = new TestRun(testIdentifier);
            var testRunInDict = ImmutableInterlocked.GetOrAdd<ITestIdentifier, ITestRun>(ref testResults, testIdentifier, created);
            if (ReferenceEquals(created, testRunInDict))
            {
                // it was newly created
                // shall we kick off the test, or wait for its accidental completion?
                //
                // OPEN QUESTION: a test that has no call to this machinery, how can it ever be depended on to completion? Probably we have to kick it off...
                // Ideally the test discoverer is aware right to prevent duplicate testing.
                // 
                //
                //
                // The simplest thing to do (with which we'll start) is just to return the task that would run the test, and, skip if appropriate

                created.TestTask.Start();
                await await created.TestTask;
            }
            else
            {
                await testRunInDict.TestTask;
                Skip.If(testRunInDict.CompletedUnsuccessfully);
            }
        }

        Task IIntertestDependencyTracker.DependsOn(ITestIdentifier[] testsIdentifiers) => this.DependsOn(testsIdentifiers.Cast<TestIdentifier>().ToArray());

        class TestRun : ITestRun
        {
            public bool CompletedUnsuccessfully { get; private set; }
            public bool CompletedSuccessfully { get; private set; }
            public bool Pending { get; private set; }
            public Task<Task> TestTask { get; init; } = default!;


            /// <summary>
            /// Creates a TestResult of a test that's still to be started.
            /// </summary>
            /// <returns> a task representing the test. </returns>
            public TestRun(TestIdentifier testIdentifier)
            {
                this.TestTask = new Task<Task>(() => this.RunTask(testIdentifier));
            }
            private async Task RunTask(TestIdentifier testIdentifier)
            {
                this.Pending = true;
                try
                {
                    // On JBSnorro version 0.0.12 we can use the type instead of the assembly
                    var testType = Type.GetType(testIdentifier.TypeName);
                    if (testType == null)
                    {
                        testType = TypeExtensions.FindType(testIdentifier.TypeName)
                                                 .Where(t => t.IsPublic)
                                                 .FirstOrDefault();
                        if (testType == null)
                        {
                            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                            var correctAssembly = allAssemblies.Where(a => a.GetName().Name == "tests").First();
                            var correctType = correctAssembly.GetTypes().Where(t => t.Name == testIdentifier.TypeName).ToList();
                            throw new InvalidTestConfigurationException($"The type '{testIdentifier.TypeName}' could not be found");
                        }
                    }

                    IEnumerable<Func<Task>> tests = testType.GetExecutableTestMethods($"{testIdentifier.TypeName}::{(testIdentifier.IsType ? "*" : testIdentifier.TestName)}");

                    if (EnumerableExtensions.IsEmpty(ref tests))
                    {
                        throw new InvalidTestConfigurationException($"The test identifier '{testIdentifier}' does not refer to any tests");
                    }

                    foreach (var test in tests)
                    {
                        await test();
                    }
                }
                catch
                {
                    this.CompletedUnsuccessfully = true;
                    this.Pending = false;
                    throw;
                }
                this.CompletedSuccessfully = true;
                this.Pending = false;
            }
        }
    }

}

namespace ToBeJBSnorro
{
    namespace Extensions
    {
        public static class TypeExtensions
        {
            /// <summary>
            /// Finds all types in all loaded assemblies that have the specified unqualified name.
            /// </summary>
            public static IEnumerable<Type> FindType(string name)
            {
                var typeName = name.SubstringAfterLast(".");
                var @namespace = typeName.Length == name.Length ? null : name.SubstringUntilLast(".");
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        // https://stackoverflow.com/a/67976906/308451
                        continue;
                    }
                    foreach (var type in types)
                    {
                        if (type.Name == typeName && (@namespace == null || @namespace == type.Namespace))
                        {
                            yield return type;
                        }
                    }
                }
            }
        }

        public static class StringExtensions
        {
            /// <summary> 
            /// Gets the substring of <paramref name="this"/> until the last occurrence of <paramref name="value"/>;
            /// or <paramref name="this"/> if the value was not.
            /// </summary>
            /// <param name="value"> The value to find. </param>
            public static string SubstringUntilLast(this string @this, string value)
            {
                return SubstringUntilLast(@this, value, Index.End);
            }
            /// <summary> 
            /// Gets the substring of <paramref name="this"/> until the last occurrence of <paramref name="value"/>;
            /// or <paramref name="this"/> if the value was not.
            /// </summary>
            /// <param name="value"> The value to find. </param>
            /// <param name="startIndex"> The search starting position. The search proceeds from startIndex toward the beginning of this instance. </param>
            public static string SubstringUntilLast(this string @this, string value, Index startIndex)
            {
                int i = @this.LastIndexOf(value, startIndex.GetOffset(@this.Length));
                if (i == -1)
                    return @this;
                return @this[..i];
            }
        }
    }
}
