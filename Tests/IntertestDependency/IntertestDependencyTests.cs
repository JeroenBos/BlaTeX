using JBSnorro;
using JBSnorro.Csx;
using JBSnorro.Diagnostics;
using JBSnorro.Tests.IntertestDependency;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace BlaTeX.Tests.IntertestDependency
{
    public class IntertestDependencyTests
    {
        [@Fact]
        public async Task Test_depends_on()
        {
            await this.DependsOn(nameof(EmptyTest));
        }
        [@Fact]
        public void EmptyTest()
        {
        }
        [@Fact]
        public async Task Test_depends_on_not_depending_on_a_test_throws()
        {
            await Assert.ThrowsAsync<InvalidTestConfigurationException>(() => this.DependsOn(nameof(NotATest)));
        }

#pragma warning disable xUnit1013, CA1822 
        public void NotATest() { }
#pragma warning restore xUnit1013, CA1822 // Public method should be marked as test

    }
    public class IntertestDependencyIntegrationTestsBase
    {
        protected static string TestsStartedExpected = "Starting test execution, please wait...";
        protected static string TestsStartedExpected2 = "A total of 1 test files matched the specified pattern.";
        protected static string ExpectedTally(int passed = 0, int skipped = 0, int failed = 0)
        {
            return $"{(failed == 0 ? "Passed" : "Failed")}!  - Failed:     {failed}, Passed:     {passed}, Skipped:     {skipped}";
        }

    }
    public class IntertestMSTestDependencyIntegrationTests : IntertestDependencyIntegrationTestsBase
    {
        private static string csprojContents
        {
            get
            {
                return $@"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""16.11.0"" />
    <PackageReference Include=""MSTest.TestAdapter"" Version=""2.2.7"" />
    <PackageReference Include=""MSTest.TestFramework"" Version=""2.2.7"" />
  </ItemGroup>
  <ItemGroup>
    <Reference Include=""BlaTeX.Tests"">
      <HintPath>{Assembly.GetExecutingAssembly().Location}</HintPath>
    </Reference>
  </ItemGroup>
</Project>";
            }
        }
        private static string csprojContentsXunit
        {
            get => @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>

    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""16.11.0"" />
    <PackageReference Include=""xunit"" Version=""2.4.1"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.4.3"">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

</Project>";

        }


        [@Fact]
        public async Task Test_can_run_dotnet_test_on_tmp_setup()
        {
            // Arrange
            string tmpDir = IOExtensions.CreateTemporaryDirectory();
            File.WriteAllText(Path.Combine(tmpDir, "tests.csproj"), csprojContents);
            File.WriteAllText(Path.Combine(tmpDir, "tests.cs"),
                @"
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
");

            // Act
            var output = await ProcessExtensions.WaitForExitAndReadOutputAsync(new ProcessStartInfo("dotnet", $"test \"{tmpDir}/tests.csproj\""));

            // Assert
            Contract.Assert(output.ExitCode == 0, output.ErrorOutput);
            Assert.Contains(TestsStartedExpected, output.StandardOutput);
            Assert.Contains(TestsStartedExpected2, output.StandardOutput);
            Assert.Contains(ExpectedTally(passed: 1), output.StandardOutput);
        }

        [@Fact]
        public async Task Test_can_compile_with_DependsOn()
        {
            // Arrange
            string tmpDir = IOExtensions.CreateTemporaryDirectory();
            File.WriteAllText(Path.Combine(tmpDir, "tests.csproj"), csprojContents);
            File.WriteAllText(Path.Combine(tmpDir, "tests.cs"),
                @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using JBSnorro.Tests.IntertestDependency;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            await this.DependsOn(nameof(TestMethod2));
        }
        [TestMethod]
        public void TestMethod2()
        {
        }
    }
}
");

            // Act
            var output = await ProcessExtensions.WaitForExitAndReadOutputAsync(new ProcessStartInfo("dotnet", $"test \"{tmpDir}/tests.csproj\""));

            // Assert
            Contract.Assert(output.ExitCode == 0, output.ErrorOutput);
            Assert.Contains(TestsStartedExpected, output.StandardOutput);
            Assert.Contains(TestsStartedExpected2, output.StandardOutput);
            Assert.Contains(ExpectedTally(passed: 2), output.StandardOutput);

        }
        [@Fact]
        public async Task Test_does_not_stackoverflow_DependsOn_self()
        {
        }
    }
    public class IntertestXunitDependencyIntegrationTests : IntertestDependencyIntegrationTestsBase
    {
        private static string csprojContents
        {
            get => $@"<Project Sdk=""Microsoft.NET.Sdk.Razor"">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>

    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""16.11.0"" />
    <PackageReference Include=""xunit"" Version=""2.4.2-pre.12"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.4.3"">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>

    <!-- I'm including these, see https://stackoverflow.com/a/67976906/308451 -->
    <PackageReference Include=""Microsoft.AspNetCore.Components.WebAssembly"" Version=""6.0.0-preview.6.21355.2"" />
	<PackageReference Include=""Microsoft.AspNetCore.Components.WebAssembly.DevServer"" Version=""6.0.0-preview.6.21355.2"" PrivateAssets=""all"" />
  </ItemGroup>
  <ItemGroup>
    <Reference Include=""BlaTeX.Tests"">
      <HintPath>{Assembly.GetExecutingAssembly().Location}</HintPath>
    </Reference>
  </ItemGroup>

</Project>";

        }


        [@Fact]
        public async Task Test_can_run_dotnet_test_on_tmp_setup()
        {
            // Arrange
            string tmpDir = IOExtensions.CreateTemporaryDirectory();
            File.WriteAllText(Path.Combine(tmpDir, "tests.csproj"), csprojContents);
            File.WriteAllText(Path.Combine(tmpDir, "tests.cs"),
                @"
using Xunit;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }
    }
}");

            // Act
            var output = await ProcessExtensions.WaitForExitAndReadOutputAsync(new ProcessStartInfo("dotnet", $"test \"{tmpDir}/tests.csproj\""));

            // Assert
            Contract.Assert(output.ExitCode == 0, output.ErrorOutput);
            Assert.Contains(TestsStartedExpected, output.StandardOutput);
            Assert.Contains(TestsStartedExpected2, output.StandardOutput);
            Assert.Contains(ExpectedTally(passed: 1), output.StandardOutput);
        }

        [@Fact]
        public async Task Test_can_compile_with_DependsOn()
        {
            // Arrange
            string tmpDir = IOExtensions.CreateTemporaryDirectory();
            File.WriteAllText(Path.Combine(tmpDir, "tests.csproj"), csprojContents);
            File.WriteAllText(Path.Combine(tmpDir, "tests.cs"),
                @"
using System.Threading.Tasks;
using Xunit;
using JBSnorro.Tests.IntertestDependency;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public async Task TestMethod1()
        {
            await this.DependsOn(nameof(TestMethod2));
        }
        [Fact]
        public void TestMethod2()
        {
        }
    }
}
");

            // Act
            var output = await ProcessExtensions.WaitForExitAndReadOutputAsync(new ProcessStartInfo("dotnet", $"test \"{tmpDir}/tests.csproj\""));

            // Assert
            Contract.Assert(output.ExitCode == 0, output.ErrorOutput);
            Assert.Contains(TestsStartedExpected, output.StandardOutput);
            Assert.Contains(TestsStartedExpected2, output.StandardOutput);
            Assert.Contains(ExpectedTally(passed: 2), output.StandardOutput);

        }

    }
}

