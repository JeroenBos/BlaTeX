namespace BlaTeX;

/// <summary>
/// A service that identifies parts of markup and known by what to substitute them.
/// </summary>
public interface IChildComponentMarkupService
{
    /// <summary>
    /// Gets ranges in the markup that are to be substituted by this service.
    /// </summary>
    IEnumerable<Range> Select(string markup);
    /// <summary>
    /// Given a part of markup returns the renderer.
    /// </summary>
    RenderFragment Substitute(string markupFragment);
    // IReadOnlyList<AnyParseNode> Select(IReadOnlyList<AnyParseNode> ast);
}
