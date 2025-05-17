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

    public bool Trace { get; private init; }
    public JsonSerializerOptions Options { get; }
    public IReadOnlyList<JSString> Imports { get; }
    public IReadOnlyList<KeyValuePair<Type, string>> JSDeserializableTypes { get; }

    public NodeJSRuntime(IEnumerable<JSString> imports) : this(imports, []) { }
    public NodeJSRuntime(IEnumerable<JSString> imports, IEnumerable<(Type Type, string Discriminant)> jsDeserializableIDs)
    {
        Contract.Requires(imports is not null);
        Contract.Requires(Contract.ForAll(imports, jss => File.Exists(jss.Value) || jss.Value.StartsWith("const ") || jss.Value.StartsWith("var ")), "Consider rebuilding that JS stack");
        Contract.Requires(jsDeserializableIDs is not null);
        Contract.Requires(Contract.ForAll(jsDeserializableIDs, ids => ids.Type is not null && string.IsNullOrEmpty(ids.Discriminant)));

        this.Imports = imports.ToReadOnlyList();
        this.JSDeserializableTypes = jsDeserializableIDs.Select(JBSnorro.TupleExtensions.ToKeyValuePair).ToReadOnlyList();
        this.Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        this.Options.AddKaTeXJsonConverters();
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
                                        jsIdentifiers: this.JSDeserializableTypes,
                                        options: this.Options,
                                        typeIdPropertyName: nameof(IJSSerializable.SERIALIZATION_TYPE_ID));
    }
    public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[]? args)
    {
        var (exitCode, stdOut, stdErr, debugOut) = await this.InvokeAsyncImpl(identifier, args);
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

    private static string RootFolder
    {
        get
        {
            var dir = new DirectoryInfo(Environment.CurrentDirectory);
            while (dir.Name != "BlaTeX")
            {
                dir = dir.Parent;
                if (dir == null)
                    throw new InvalidOperationException("Cannot find project root folder");
            }
            return dir.FullName;
        }
    }
}
