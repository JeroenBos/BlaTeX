using BlaTeX.JSInterop.KaTeX.Syntax;

namespace BlaTeX;

// this is a .cs file rather than .razor to reuse the renderer of the base class
/// <summary>
/// When provided to a KaTeX component, replaces all blazor-styled tags and arguments by child components.
/// Examples of shich blazor-styled tags are:
/// - blatex:name
/// - blatex:name=arg
/// - blatex:name=(arg1,arg2)
/// - blatex:name="many more things are possible after the ="
///
/// Their context is e.g. &lt;div blatex:a=@(b)&gt;gtText&lt;/div&gt;
/// </summary>
public partial class BlazorChildComponentMarkupService : IChildComponentMarkupService
{
    /// <summary> All child fragments will be placed in this collection. </summary>
    public IReadOnlyDictionary<string, RenderFragment<string>> Fragments { get; } = new Dictionary<string, RenderFragment<string>>();

    private readonly IReadOnlyList<(string Opening, string Closing)> Delimiters = [
        ("\"", "\""),
        ("(", ")"),
        ("{", "}"),
        ("'", "'"),
        ("@(", ")"),
    ];
    public const string BLATEX_ATTR_PREFIX = "blatex:";
    public IEnumerable<Range> Select(string markup)
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
            SkipWhitespace(ref endIndex);

            int? valueStartIndex = null;

            if (IsAt("="))
            {
                // parse the value of the attribute  (which is assumed to start with = and an opening delimiter)
                // TODO: fix that closing delimiters between "" are ignored
                int identifierEndIndex = endIndex;
                endIndex += "=".Length;

                SkipWhitespace(ref endIndex);

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
            void SkipWhitespace(ref int endIndex)
            {
                while (endIndex < markup.Length && char.IsWhiteSpace(markup[endIndex]))
                    endIndex++;
            }
        }
    }
    public RenderFragment Substitute(string markupFragment)
    {
        const string separator = "=";
        int keyEndIndex = markupFragment.IndexOf(separator);
        int valueStartIndex;
        if (keyEndIndex == -1)
        {
            keyEndIndex = markupFragment.Length;
            valueStartIndex = markupFragment.Length;
        }
        else
        {
            valueStartIndex = keyEndIndex + separator.Length;
        }

        string key = markupFragment[BLATEX_ATTR_PREFIX.Length..keyEndIndex];
        string value = markupFragment[valueStartIndex..].Trim();
        if (this.Fragments.TryGetValue(key, out RenderFragment<string>? renderFragment))
        {
            return renderFragment(value);
        }
        else
        {
            throw new Exception($"Unhandled render fragment with name '{key}'");
        }
    }

    public virtual IReadOnlyList<AnyParseNode> Select(IReadOnlyList<AnyParseNode> ast)
    {
        return ast;
    }
    // public override async Task SetParametersAsync(ParameterView parameters)
    // {
    //     var d = parameters.ToDictionary();
    //     // collect all parameters of type RenderFragment or RenderFragment<string>:
    //     this.Fragments = d.Select(pair => KeyValuePair.Create(pair.Key, AsRenderFragment(pair.Value)!))
    //                       .Where(pair => pair.Value != null)
    //                       .ToDictionary(StringComparer.OrdinalIgnoreCase);

    //     // only pass along keys for base type:
    //     var baseParameters = parameters.FilterKeys(typeof(KaTeX));
    //     int baseCount = baseParameters.ToDictionary().Count;
    //     await base.SetParametersAsync(baseParameters);

    //     if (baseCount + this.Fragments.Count != d.Count)
    //         throw new ArgumentException("Extra argument specified");

    //     static RenderFragment<string>? AsRenderFragment(object obj)
    //     {
    //         return obj switch
    //         {
    //             RenderFragment<string> fragment => fragment,
    //             RenderFragment uncontextualFragment => (_ => uncontextualFragment),
    //             _ => null
    //         };
    //     }
    // }
}

[Serializable]
public class SyntaxError : Exception
{
    public static readonly SyntaxError OutOfRange = new SyntaxError("Syntax error: end of string encountered");
    public SyntaxError() { }
    public SyntaxError(string message) : base(message) { }
    public SyntaxError(string message, Exception inner) : base(message, inner) { }
}
