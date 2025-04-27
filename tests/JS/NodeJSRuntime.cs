using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using JBSnorro.Text;
using JBSnorro.Csx;
using JBSnorro.Csx.Node;

namespace BlaTeX.Tests;

public class NodeJSRuntime : IJSRuntime
{
    public static JSString DefaultKaTeXPath { get; } = JSString.Escape_Unsafe($"const katex = require('{Path.Join(RootFolder, "js", "node_modules", "@jeroenbos", "katex", "dist", "katex.min.js")}');", escapeSingleQuotes: false, escapeDoubleQuotes: false);
    public static NodeJSRuntime CreateDefault()
    {
        return new NodeJSRuntime(DefaultKaTeXPath.ToSingleton()) { Trace = true };
    }

    public bool Trace { get; init; }
    public JsonSerializerOptions Options { get; }
    public IReadOnlyList<JSString> Imports { get; }
    public IReadOnlyList<KeyValuePair<Type, string>> IDs { get; }
    public NodeJSRuntime(IEnumerable<JSString> imports, IEnumerable<(Type, string)>? jsDeserializableIDs = null, JsonSerializerOptions? options = null)
    {
        this.Imports = imports?.ToReadOnlyList() ?? EmptyCollection<JSString>.ReadOnlyList;
        Contract.Requires(Contract.ForAll(imports ?? [], jss => File.Exists(jss.Value) || jss.Value.StartsWith("const ") || jss.Value.StartsWith("var ")), "Consider rebuilding that JS stack");
        this.IDs = jsDeserializableIDs?.Select(t => KeyValuePair.Create(t.Item1, t.Item2)).ToReadOnlyList() ?? EmptyCollection<KeyValuePair<Type, string>>.ReadOnlyList;
        if (options == null)
        {
            this.Options = new JsonSerializerOptions();
            this.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            this.Options.AddKaTeXJsonConverters();
            this.Options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        }
        else
        {
            this.Options = options;
        }
    }

    internal async Task<DebugProcessOutput> InvokeAsyncImpl(string identifier, params object?[]? args)
    {
        // do the serialization before the built-in does it (which will ignore JSSourceCodes)
        // but that one doesn't work because I can't get it to work recursively, see HACK id=0
        var original_args = args;
        if (args != null)
            for (int i = 0; i < args.Length; i++)
                if (args[i] == null)
                    throw new ArgumentNullException($"args[{i}]. Use JSSourceCode.Null or JSSourceCode.Undefined instead.");

        var mappedArgs = args?.Map(arg => arg as JSSourceCode ?? new JSSourceCode(JsonSerializer.Serialize(arg, this.Options)));
        var identifierObj = new JSSourceCode(identifier);

        var jsRunner = new JSProcessRunner(INodePathResolver.FromCommand());
        return await jsRunner.ExecuteJS(this.Imports,
                                        identifier: identifierObj,
                                        arguments: mappedArgs,
                                        jsIdentifiers: this.IDs,
                                        options: this.Options,
                                        typeIdPropertyName: nameof(IJSSerializable.SERIALIZATION_TYPE_ID));
    }
    public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args)
    {
        var (exitCode, stdOut, stdErr, debugOut) = await this.InvokeAsyncImpl(identifier, args);
        Console.WriteLine(stdOut);
        Console.WriteLine(stdErr);
        if (stdErr != "")
        {
            System.Diagnostics.Debug.WriteLine("stdErr:");
            System.Diagnostics.Debug.WriteLine(stdErr);
        }
        if (this.Trace)
        {
            System.Diagnostics.Debug.WriteLine("stdOut:");
            System.Diagnostics.Debug.WriteLine(stdOut);
        }
        if (stdOut == "")
            return default!;
        try
        {
            Contract.Assert(!stdOut.Contains(nameof(IJSSerializable.SERIALIZATION_TYPE_ID)));
            var result = JsonSerializer.Deserialize<TValue>(stdOut, this.Options);
            if (result == null)
                throw new JsonException("According to IJSRuntime.InvokeAsync, we're not allowed to return null here");
            return result;
        }
        catch (JsonException)
        {
            Console.WriteLine(stdOut);
            throw;
        }
    }

    ValueTask<TValue> IJSRuntime.InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        return this.InvokeAsync<TValue>(identifier, args);
    }

    public static string RootFolder
    {
        get
        {
            throw new Exception(System.Text.Encoding.Default.EncodingName);
            var dir = new DirectoryInfo(Environment.CurrentDirectory);
            while (dir.Name.ToLowerInvariant() != "blatex") // relevant since the systemwide UTF8 setting
            {
                dir = dir.Parent;
                if (dir == null)
                    throw new InvalidOperationException("Cannot find project root folder");
            }
            return dir.FullName;
        }
    }
}
