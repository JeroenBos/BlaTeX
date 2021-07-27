using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using JBSnorro.Diagnostics;
using BlaTeX.Components;

namespace BlaTeX.Tests
{
	public class BlazorStyledChildComponentsTests
	{
		// returns the range of the entire `blaxex:name=value`, where `=value` is optional
		private static IEnumerable<Range> Selector(string markup) => new BlazorChildComponentMarkupService().Select(markup);

		[Fact]
		public void EmptyMarkup()
		{
			const string markup = "";

			var ranges = Selector(markup).ToList();

			Contract.Assert(ranges.Count == 0);
		}

		[Fact]
		public void SimpleMarkup()
		{
			const string markup = "blatex:a";
			var ranges = Selector(markup).ToList();

			Contract.AssertSequenceEqual(ranges, new[] { new Range(0, markup.Length) });
		}

		[Fact]
		public void SimpleMarkupWithValue()
		{
			const string markup = "blatex:a=b";
			var ranges = Selector(markup).ToList();

			Contract.AssertSequenceEqual(ranges, new[] { new Range(0, markup.Length) });
		}

		[Fact]
		public void SimpleMarkupWithDelimitedValue()
		{
			const string markup = "blatex:a='b'";
			var ranges = Selector(markup).ToList();

			Contract.AssertSequenceEqual(ranges, new[] { new Range(0, markup.Length) });
		}

		[Fact]
		public void SimpleMarkupWithDelimitedValueDoesntIncludeTrailingWhitespace()
		{
			const string markup = "blatex:a='b' ";
			var ranges = Selector(markup).ToList();

			Contract.AssertSequenceEqual(ranges, new[] { new Range(0, markup.Length - 1) });
		}
		[Fact]
		public void SimpleMarkupWithParenthesesDelimiters()
		{
			const string markup = "blatex:a=(b)";
			var ranges = Selector(markup).ToList();

			Contract.AssertSequenceEqual(ranges, new[] { new Range(0, markup.Length) });
		}
		[Fact]
		public void SimpleMarkupWithParenthesesDelimitersDoesntIncludeTrailingWhitespace()
		{
			const string markup = "blatex:a=(b) ";
			var ranges = Selector(markup).ToList();

			Contract.AssertSequenceEqual(ranges, new[] { new Range(0, markup.Length - 1) });
		}
		[Fact]
		public void SimpleMarkupWithBlazorParenthesesDelimiters()
		{
			const string markup = "blatex:a=@(b)";
			var ranges = Selector(markup).ToList();

			Contract.AssertSequenceEqual(ranges, new[] { new Range(0, markup.Length) });
		}
		[Fact]
		public void SimpleMarkupWithBlazorParenthesesDelimitersDoesntIncludeTrailingWhitespace()
		{
			const string markup = "blatex:a=@(b) ";
			var ranges = Selector(markup).ToList();

			Contract.AssertSequenceEqual(ranges, new[] { new Range(0, markup.Length - 1) });
		}

		[Fact]
		public void EmbeddedSimpleMarkupWithBlazor()
		{
			const string markup = "<div blatex:a=@(b)/>";
			var ranges = Selector(markup).ToList();

			Contract.AssertSequenceEqual(ranges, new[] { new Range(markup.IndexOf("bla"), markup.IndexOf("/>")) });
		}

		[Fact]
		public void EmbeddedSimpleMarkupWithBlazor2()
		{
			const string markup = "<div blatex:a=@(b)>Text</div>";
			var ranges = Selector(markup).ToList();

			Contract.AssertSequenceEqual(ranges, new[] { new Range(markup.IndexOf("bla"), markup.IndexOf(">T")) });
		}
	}

}
