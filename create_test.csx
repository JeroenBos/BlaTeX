// real reference:
#r "./Tests/bin/Debug/netcoreapp3.1/BlaTeX.Tests.dll"
// path hints:
#r "./Tests/bin/Debug/netcoreapp3.1/BlaTeX.dll"
#r "./Tests/bin/Debug/netcoreapp3.1/JBSnorro.dll"
#r "./Tests/bin/Debug/netcoreapp3.1/Microsoft.JSInterop.dll"
using BlaTeX.Tests;
using System.Xml.Linq;
using JBSnorro;

if (Args.Count != 1)
    throw new ArgumentException("Expected one argument.");

string input = Args[0];
Console.WriteLine($"Rendering \"{input}\":");

var imports = new[] { $"{Program.RootFolder}/wwwroot/js/blatex_wrapper.js" };
var result = await new NodeJSRuntime(imports).InvokeAsync<string>("blatex_wrapper.default.renderToString", new[] { input }).AsTask();

bool wrap = true;
if (wrap)
{
    Console.WriteLine("@inherits KaTeXTestComponentBase");
    Console.WriteLine();
    result = $"<KaTeXTest math=\"{input}\">{result}</KaTeXTest>";
}
var formatted = XElement.Parse(result).ToString();
Console.WriteLine(formatted);
