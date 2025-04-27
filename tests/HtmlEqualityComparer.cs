using AngleSharp.Diffing.Core;
using AngleSharp.Dom;
using Bunit.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Linq;

namespace BlaTeX;

public static class HtmlEqualityComparer
{
    private const string defaultMessage = "Html unequal";

    public static IReadOnlyList<IDiff> ComputeDifference(INodeList expected, INodeList actual)
    {
        return actual.CompareTo(expected);
    }
    public static IReadOnlyList<IDiff> ComputeDifference(string expected, INodeList actual, out INodeList expectedNodes)
    {
        using var parser = new BunitHtmlParser();
        return ComputeDifference(expected, actual, out expectedNodes);
    }
    public static IEnumerable<IDiff> ComputeDifference(string expected, INodeList actual, BunitHtmlParser parser, out INodeList expectedNodes)
    {
        expectedNodes = parser.Parse(expected);
        return ComputeDifference(expectedNodes, actual);
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
    public static void AssertEqual(IRenderedFragment expected, IRenderedFragment actual, string message = defaultMessage)
    {
        AssertEqual(expected.Nodes, actual.Nodes, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(IRenderedFragment expected, INodeList inputNodes, string message = defaultMessage)
    {
        AssertEqual(expected.Nodes, inputNodes, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(string expected, INodeList actual, TestServiceProvider services, string message = defaultMessage)
    {
        var parser = services.GetRequiredService<BunitHtmlParser>();
        AssertEqual(expected, actual, parser, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(string expected, IRenderedFragment actual, TestServiceProvider services, string message = defaultMessage)
    {
        var parser = services.GetRequiredService<BunitHtmlParser>();
        AssertEqual(expected, actual.Nodes, parser, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(string expected, IRenderedFragment actual, BunitHtmlParser parser, string message = defaultMessage)
    {
        AssertEqual(expected, actual.Nodes, parser, message);
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
    public static void AssertEqual(string expected, INodeList actual, BunitHtmlParser parser, string message = defaultMessage)
    {
        AssertEqual(parser.Parse(expected), actual, message);
    }
    /// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
    [DebuggerHidden]
    public static void AssertEqual(INodeList expected, INodeList actual, string message = defaultMessage)
    {
        HtmlEqualException? exception = ComputeException(expected, actual, message);
        if (exception != null)
            throw exception;
    }
    /// <summary> Gets a detailed error if the specified strings don't represent equivalent HTML. </summary>
    internal static HtmlEqualException? ComputeException(INodeList expected, INodeList actual, string message = defaultMessage)
    {
        var diffs = ComputeDifference(expected, actual);

        if (diffs.Any())
            return new HtmlEqualException(diffs, expected, actual, message);

        return null;
    }

    [DebuggerHidden]
    public static IEnumerable<IDiff> ComputeDifference(string x, string y) => ComputeDifference(x, y, out var _);
}
