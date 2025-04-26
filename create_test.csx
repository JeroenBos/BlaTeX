// real reference:
#r "./tests/bin/Debug/netcoreapp3.1/BlaTeX.Tests.dll"
// path hints:
#r "./tests/bin/Debug/netcoreapp3.1/BlaTeX.dll"
#r "./tests/bin/Debug/netcoreapp3.1/JBSnorro.dll"
#r "./tests/bin/Debug/netcoreapp3.1/Microsoft.JSInterop.dll"
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
	Console.WriteLine();
	Console.WriteLine("@inherits KaTeXTestComponentBase");
	Console.WriteLine();
	result = $"<KaTeXTest math=\"{input}\">{result}</KaTeXTest>";
}

static string Format(string s)
{
	return XElement.Parse(s).ToString();
}
var formatted = Preserve("\n", Format)(result);
Console.WriteLine(formatted);








Func<string, string> Preserve(string stringToPreserve, Func<string, string> map)
{
	string f(string s)
	{
		const string escapeSequence = "escapestring";
		s = s.Replace(stringToPreserve, escapeSequence);
		s = map(s);
		s = s.Replace(escapeSequence, stringToPreserve);
		return s;
	}
	return f;
}
