namespace BlaTeX.JSInterop;

public class NodeJSException : JSException
{
    public string? DebugOutput { get; init; }

    public NodeJSException(string message) : base(message) { }
    public NodeJSException(string message, Exception innerException) : base(message, innerException) { }
    public NodeJSException(string message, string debugOutput) : base(message)
    {
        this.DebugOutput = debugOutput;
    }
    public NodeJSException(string message, Exception innerException, string debugOutput) : base(message, innerException)
    {
        this.DebugOutput = debugOutput;
    }
}
