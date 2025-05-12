
/// <summary>
/// Represents substituting of a literal by another simple renderer.
/// </summary>
public class LiteralChildComponentMarkupService : IChildComponentMarkupService
{
    private readonly string literal;
    private readonly RenderFragment substitute;

    public LiteralChildComponentMarkupService(string literal, RenderFragment substitute)
    {
        this.literal = literal ?? throw new ArgumentNullException(nameof(literal));
        this.substitute = substitute ?? throw new ArgumentNullException(nameof(substitute));
    }

    public IEnumerable<Range> Select(string markup)
    {
        int previousIndex = markup.IndexOf("katex-html"); // skip "katex-mathml"
        Console.WriteLine($"previousIndex={previousIndex}");
        while (true)
        {
            int index = markup.IndexOf(literal, previousIndex);
            if (index == -1)
            {
                yield break;
            }
            yield return new Range(index, index + literal.Length);
            Console.WriteLine($"index={index}");
            Console.WriteLine($"Replacing={markup[index..(index + literal.Length)]}");

            previousIndex = index + literal.Length;
        }
    }

    public RenderFragment Substitute(string markupFragment)
    {
        return substitute;
    }
}
