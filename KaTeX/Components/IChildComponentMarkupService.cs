using BlaTeX.JSInterop.KaTeX.Syntax;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace BlaTeX
{
	public interface IChildComponentMarkupService
	{
		IEnumerable<Range> Select(string markup);
		RenderFragment Substitute(string markupFragment);
		IReadOnlyList<AnyParseNode> Select(IReadOnlyList<AnyParseNode> ast);
	}
}