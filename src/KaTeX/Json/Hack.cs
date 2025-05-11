namespace BlaTeX.JSInterop.KaTeX;

internal class IJSSerializableState
{
    // HACK HACK HACK id=0 because I can't get the System.Text.Json.JsonSerializer/Converter/Options to do what I want.... 
    // which is to add a the property 'SERIALIZATION_TYPE_ID' to the following list of types:
    internal static IReadOnlyList<Type> convertibleTypes = [
        // the only classes in KaTeX that implement VirtualNode (in js) are SvgNode, PathNode, LineNode, MathDomNode, 
        // and the interface HtmlDomNode, which is implemented only by Span, Anchor, SymbolNode and DocumentFragment 
        // Complicatedly, DocumentFragment also derives from MathDomNode. But we'll see that when we get to it

        // uncomment these when they're available:
        // typeof(DocumentFragment),
        typeof(IMathDomNode),
        // typeof(SvgNode),
        // typeof(PathNode),
        // typeof(LineNode),
        typeof(ISpan<>),
        // typeof(SymbolNode),
        // typeof(Anchor),
    ];
}
internal interface IJSSerializable
{
    public static string SERIALIZATION_TYPE_ID_Impl(IJSSerializable @this)
    {
        if (@this == null)
            throw new ArgumentNullException(nameof(@this));


        int index = IJSSerializableState.convertibleTypes.IndexOf(t => t.IsAssignableFrom(@this.GetType()));
        if (index == -1)
            throw new ArgumentException($"The type '{@this.GetType().FullName}' is not JS serializable");
        return index.ToString();
    }
    string SERIALIZATION_TYPE_ID { get; }
}
