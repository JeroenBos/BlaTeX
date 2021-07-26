#nullable enable
using AngleSharp.Diffing.Core;
using AngleSharp.Dom;
using Bunit;
using Bunit.Diffing;
using Bunit.Rendering;
using JBSnorro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlaTeX
{
	public class HtmlEqualityComparer : IDifferenceComparer<string, IDiff>
	{
		private const string defaultMessage = "Html unequal";
		public static HtmlEqualityComparer Instance { get; } = new HtmlEqualityComparer();

		[DebuggerHidden]
		public static IEnumerable<IDiff> ComputeDifference(string x, string y, out (INodeList X, INodeList Y) nodes)
		{
			using var parser = new BunitHtmlParser();
			nodes = (parser.Parse(x), parser.Parse(y));
			var diffs = nodes.X.CompareTo(nodes.Y);
			return diffs;
		}
		/// <summary> Throws a detailed error if the specified strings don't represent equivalent HTML. </summary>
		[DebuggerHidden]
		public static void AssertEqual(string expected, string input, string message = defaultMessage)
		{
			HtmlEqualException? exception = ComputeException(expected, input, message);
			if (exception != null)
				throw exception;
		}
		/// <summary> Gets a detailed error if the specified strings don't represent equivalent HTML. </summary>
		[DebuggerHidden]
		internal static HtmlEqualException? ComputeException(string expected, string input, string message = defaultMessage)
		{
			var diffs = ComputeDifference(expected, input, out (INodeList Expected, INodeList Input) nodes);

			if (diffs.Any())
				return new HtmlEqualException(diffs, nodes.Expected, nodes.Input, message);

			return null;
		}



		protected HtmlEqualityComparer() { }


		[DebuggerHidden]
		IEnumerable<IDiff> IDifferenceComparer<string, IDiff>.ComputeDifference(string x, string y) => ComputeDifference(x, y);

		[DebuggerHidden]
		public static IEnumerable<IDiff> ComputeDifference(string x, string y) => ComputeDifference(x, y, out var _);
	}
}
