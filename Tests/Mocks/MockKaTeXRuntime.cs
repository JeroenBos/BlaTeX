using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlaTeX.JSInterop.KaTeX.Syntax;
using BlaTeX.JSInterop.KaTeX;
using JBSnorro;

namespace BlaTeX.Tests.Mocks
{
	record MockKaTeXRuntime : IKaTeXRuntime
	{
		public string RenderToStringReturnValue { get; init; } = "<div diff:ignore></div>";
		public IReadOnlyList<AnyParseNode> ParseReturnValue { get; init; } = DummyParseNodeList.Instance;


		public MockKaTeXRuntime With(Option<string> renderToStringReturnValue = default,
									 Option<IReadOnlyList<AnyParseNode>> parseReturnValue = default)
		{
			var @this = this;
			if (renderToStringReturnValue.HasValue)
				@this = @this with { RenderToStringReturnValue = renderToStringReturnValue.Value };
			if (parseReturnValue.HasValue)
				@this = @this with { ParseReturnValue = parseReturnValue.Value };
			return @this;
		}

		public Task<IReadOnlyList<AnyParseNode>> Parse(string math) => Task.FromResult(ParseReturnValue);
		public Task<string> RenderToString(string math) => Task.FromResult(RenderToStringReturnValue);
		public Task<string> RenderToString(IReadOnlyList<AnyParseNode> tree, string math = null) => Task.FromResult(RenderToStringReturnValue);
		public Task<string> ToMarkup(VirtualNode node) => Task.FromResult(RenderToStringReturnValue);

		public Task<HtmlDomNode> RenderToDom(string math) => throw new NotImplementedException();

		class DummyParseNodeList : ReadOnlyCollection<AnyParseNode>
		{
			public static DummyParseNodeList Instance = new();
			private DummyParseNodeList() : base(Array.Empty<AnyParseNode>()) { }
		}
	}
}
