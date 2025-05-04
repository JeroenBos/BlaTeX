namespace BlaTeX;

[Serializable]
public class SyntaxError : Exception
{
    public SyntaxError() { }
    public SyntaxError(string message) : base(message) { }
    public SyntaxError(string message, Exception inner) : base(message, inner) { }
}
