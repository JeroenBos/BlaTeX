using BlaTeX.JSInterop.KaTeX;
using BlaTeX.JSInterop.KaTeX.Syntax;
using JBSnorro;
using JBSnorro.Collections.ObjectModel;
using JBSnorro.Diagnostics;
using JBSnorro.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using static JBSnorro.Diagnostics.Contract;

namespace BlaTeX.Tests
{
	public class KaTeXTypesSerializationTests
	{
		private NodeJSRuntime jsRuntime;
		private readonly IKaTeX KaTeX;
		public KaTeXTypesSerializationTests()
		{
			jsRuntime = NodeJSRuntime.CreateDefault();
			KaTeX = IKaTeX.Create(jsRuntime);
		}

		[Fact]
		public async Task SerializeSqrtParseNode()
		{
			var node = new _AnyParseNode(NodeType.Sqrt, Mode.Math, new _SourceLocation(0, 4));
			string s = await KaTeX.RenderToString(new[] { node });
			Contract.Assert(s != null);
		}
	}
}
