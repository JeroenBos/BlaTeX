using AngleSharp.Diffing.Core;
using AngleSharp.Dom;
using Bunit.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;

namespace BlaTeX;

public class HtmlEqualityComparer : IDifferenceComparer<string, IDiff>
{
    private const string defaultMessage = "Html unequal";
    public static HtmlEqualityComparer Instance { get; } = new HtmlEqualityComparer();

    public static IReadOnlyList<IDiff> ComputeDifference(INodeList expected, INodeList renderedInput)
    {
        return renderedInput.CompareTo(expected);
    }
    public static IReadOnlyList<IDiff> ComputeDifference(string expected, INodeList renderedInput, out INodeList expectedNodes)
    {
        using var parser = new BunitHtmlParser();
        return ComputeDifference(expected, renderedInput, out expectedNodes);
    }
    public static IEnumerable<IDiff> ComputeDifference(string expected, INodeList renderedInput, BunitHtmlParser parser, out INodeList expectedNodes)
    {
        expectedNodes = parser.Parse(expected);
        return ComputeDifference(expectedNodes, renderedInput);
    }
    public static IReadOnlyList<IDiff> ComputeDifference(string expected, string input, out (INodeList Expected, INodeList Input) nodes)
    {
        using var parser = new BunitHtmlParser();
        return ComputeDifference(expected, input, parser, out nodes);
    }
    public static IReadOnlyList<IDiff> ComputeDifference(string expected, string input, BunitHtmlParser parser, out (INodeList Expected, INodeList Input) nodes)
    {
        nodes = (parser.Parse(expected), parser.Parse(input));

        return nodes.Input.CompareTo(nodes.Expected);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(IRenderedFragment expectedFragment, IRenderedFragment renderedFragment, string message = defaultMessage)
    {
        AssertEqual(expectedFragment.Nodes, renderedFragment.Nodes, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(IRenderedFragment expectedFragment, INodeList inputNodes, string message = defaultMessage)
    {
        AssertEqual(expectedFragment.Nodes, inputNodes, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(string expected, INodeList renderedNodes, TestServiceProvider services, string message = defaultMessage)
    {
        var parser = services.GetRequiredService<BunitHtmlParser>();
        AssertEqual(expected, renderedNodes, parser, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(string expected, IRenderedFragment renderedFragment, TestServiceProvider services, string message = defaultMessage)
    {
        var parser = services.GetRequiredService<BunitHtmlParser>();
        AssertEqual(expected, renderedFragment.Nodes, parser, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(string expected, IRenderedFragment renderedFragment, BunitHtmlParser parser, string message = defaultMessage)
    {
        AssertEqual(expected, renderedFragment.Nodes, parser, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(string expected, string input, string message = defaultMessage)
    {
        using var parser = new BunitHtmlParser();
        AssertEqual(expected, parser.Parse(input), parser, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(string expected, string input, TestServiceProvider services, string message = defaultMessage)
    {
        var parser = services.GetRequiredService<BunitHtmlParser>();
        AssertEqual(expected, input, parser, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(string expected, string input, BunitHtmlParser parser, string message = defaultMessage)
    {
        AssertEqual(expected, parser.Parse(input), parser, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(string expected, INodeList renderedInput, BunitHtmlParser parser, string message = defaultMessage)
    {
        AssertEqual(parser.Parse(expected), renderedInput, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(INodeList expectedNodes, INodeList renderedInput, string message = defaultMessage)
    {
        HtmlEqualException? exception = ComputeException(expectedNodes, renderedInput, message);
        if (exception != null)
            throw exception;
    }
    /// <summary> Gets a detailed error if the specified strings don't represent equivalent HTML. </summary>
    internal static HtmlEqualException? ComputeException(INodeList expectedNodes, INodeList inputNodes, string message = defaultMessage)
    {
        var diffs = ComputeDifference(expectedNodes, inputNodes);

        if (diffs.Any())
            return new HtmlEqualException(diffs, expectedNodes, inputNodes, message);

        return null;
    }



    protected HtmlEqualityComparer() { }


    [DebuggerHidden]
    IEnumerable<IDiff> IDifferenceComparer<string, IDiff>.ComputeDifference(string x, string y) => ComputeDifference(x, y);

    [DebuggerHidden]
    public static IEnumerable<IDiff> ComputeDifference(string x, string y) => ComputeDifference(x, y, out var _);
}
