#nullable enable
using JBSnorro;
using JBSnorro.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using JBSnorro.Diagnostics;
using System.Runtime.Serialization;

namespace BlaTeX
{
    // this is a .cs file rather than .razor to reuse the renderer of the base class
    public partial class ComponentizedKaTeX : BlaTeX.Pages.KaTeX
    {
        private readonly IReadOnlyList<(string Opening, string Closing)> Delimiters = new[] {
            ("\"", "\""),
            ("(", ")"),
            ("{", "}"),
            ("'", "'"),
            ("@(", ")"),
        };
        public const string BLATEX_ATTR_PREFIX = "blatex:";
        internal protected override IEnumerable<Range> Selector(string markup)
        {
            var delimiterStack = new Stack<string>();
            int skipUntil = -1;
            foreach (var prefixStartIndex in markup.IndicesOf(BLATEX_ATTR_PREFIX)) // TODO: , StringComparison.OrdinalIgnoreCase))
            {
                if (prefixStartIndex < skipUntil)
                    continue; // skip nested prefixes

                int endIndex = prefixStartIndex;
                endIndex += BLATEX_ATTR_PREFIX.Length;

                for (; endIndex < markup.Length; endIndex++)
                {
                    if (isIdentifierChar(markup[endIndex]))
                        continue;
                    else
                        break;
                }
                // endIndex being equal to markup.Length is fine here (although I don't think it will ever happen when embedded in html)

                while (char.IsWhiteSpace(markup[endIndex]))
                    endIndex++;

                int? valueStartIndex = null;

                if (IsAt("="))
                {
                    // parse the value of the attribute  (which is assumed to start with = and an opening delimiter)
                    // TODO: fix that closing delimiters between "" are ignored
                    int identifierEndIndex = endIndex;
                    endIndex += "=".Length;

                    while (char.IsWhiteSpace(markup[endIndex]))
                        endIndex++;

                    valueStartIndex = endIndex;

                    do
                    {
                        if (endIndex >= markup.Length)
                            throw SyntaxError.OutOfRange;

                        if (delimiterStack.Count != 0 && IsAt(delimiterStack.Peek()))
                        {
                            // close delimiter
                            endIndex += delimiterStack.Peek().Length;
                            delimiterStack.Pop();
                        }
                        else
                        {
                            bool openedDelimiter = false;
                            foreach (var pair in this.Delimiters)
                            {
                                if (IsAt(pair.Opening))
                                {
                                    // open delimiter
                                    delimiterStack.Push(pair.Closing);
                                    endIndex += pair.Opening.Length;
                                    openedDelimiter = true;
                                    break;
                                }
                                if (IsAt(pair.Closing))
                                {
                                    throw new Exception($"Syntax error: Unexpected closing delimiter '{pair.Closing}'");
                                }
                            }
                            if (!openedDelimiter)
                            {
                                // consume non-delimiter character
                                endIndex++;
                            }
                        }
                    } while (delimiterStack.Count != 0);

                    bool noOpeningDelimiterAfterEqualsSign = valueStartIndex == endIndex;
                    if (noOpeningDelimiterAfterEqualsSign)
                    {
                        while (isIdentifierChar(markup[endIndex]))
                            endIndex++;
                    }
                }

                yield return new Range(prefixStartIndex, endIndex);

                bool IsAt(string s) => markup.EqualsAt(s, endIndex);
                bool isIdentifierChar(char c) => char.IsLetterOrDigit(c) || c.IsAnyOf('_', '-');
            }
        }
        internal protected override RenderFragment Substitute(string markupFragment)
        {
            throw new InvalidOperationException("This method must be overridden");
        }
    }

    [System.Serializable]
    public class SyntaxError : Exception
    {
        public static readonly SyntaxError OutOfRange = new SyntaxError("Syntax error: end of string encountered");
        public SyntaxError() { }
        public SyntaxError(string message) : base(message) { }
        public SyntaxError(string message, Exception inner) : base(message, inner) { }
    }
}