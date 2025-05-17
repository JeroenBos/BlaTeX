
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

    public IEnumerable<(Range, RenderFragment)> SelectSubstitutions(string markup)
    {
        int previousIndex = 0;
        while (true)
        {
            int index = markup.IndexOf(literal, previousIndex);
            if (index == -1)
            {
                yield break;
            }
            yield return (new Range(index, index + literal.Length), substitute);
            previousIndex = index + literal.Length;
        }
    }
}
