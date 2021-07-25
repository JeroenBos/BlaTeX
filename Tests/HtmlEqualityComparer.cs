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
		public static HtmlEqualityComparer Instance { get; } = new HtmlEqualityComparer();


		[DebuggerHidden]
		public static IEnumerable<IDiff> ComputeDifference(string x, string y, out (INodeList X, INodeList Y) nodes)
		{
			using var parser = new BunitHtmlParser();
			nodes = (parser.Parse(x), parser.Parse(y));
			var diffs = nodes.X.CompareTo(nodes.Y);
			return diffs;
		}
		/// <summary> Throws a detailed error message if the specified strings don't represent equivalent HTML. </summary>
		[DebuggerHidden]
		public static void AssertEqual(string expected, string input, string message = "Html unequal")
		{
			var diffs = HtmlEqualityComparer.ComputeDifference(expected, input, out (INodeList Expected, INodeList Input) nodes);

			if (diffs.Any())
				throw new HtmlEqualException(diffs, nodes.Expected, nodes.Input, message);
		}




		protected HtmlEqualityComparer() { }
		

		[DebuggerHidden]
		IEnumerable<IDiff> IDifferenceComparer<string, IDiff>.ComputeDifference(string x, string y) => ComputeDifference(x, y);
		
		[DebuggerHidden]
		public static IEnumerable<IDiff> ComputeDifference(string x, string y) => ComputeDifference(x, y, out var _);
	}
}
