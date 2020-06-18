#nullable enable
using System.Threading.Tasks;
using Bunit.RazorTesting;
using Xunit;

public abstract class MyRazorTestBase : RazorTestBase
{
    [RazorTest]
    /// <summary>Run the test logic of the Bunit.RazorTesting.RazorTestBase.</summary>
    // Applies the Fact attribute to this class. Method has a different name because otherwise "error xUnit1024"
    public Task RunRazorTest() => base.RunTest();


    /// <inheritdoc/>
    public override string? DisplayName => this.GetType().Name;

}