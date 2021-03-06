#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using System.Text.Json;
using JBSnorro;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JBSnorro.Extensions;
using JBSnorro.Diagnostics;
using System.Collections.Generic;
using BlaTeX.JSInterop;
using System.Text.Encodings.Web;
using BlaTeX.JSInterop.KaTeX;

namespace BlaTeX.Tests
{
    public class NodeJSRuntime : IJSRuntime
    {
        public static NodeJSRuntime CreateDefault()
        {
            return new NodeJSRuntime($"{Program.RootFolder}/wwwroot/js/blatex_wrapper.js".ToSingleton());
        }

        public JsonSerializerOptions Options { get; }
        public IReadOnlyList<string> Imports { get; }
        public IReadOnlyList<KeyValuePair<Type, string>> IDs { get; }
        public NodeJSRuntime(IEnumerable<string> imports, IEnumerable<(Type, string)>? jsDeserializableIDs = null, JsonSerializerOptions? options = null)
        {
            this.Imports = imports?.ToReadOnlyList() ?? EmptyCollection<string>.ReadOnlyList;
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

        internal async Task<(int ExitCode, string StandardOutput, string ErrorOutput)> InvokeAsyncImpl(string identifier, params object[]? args)
        {
            // do the serialization before the built-in does it (which will ignore JSStrings)
            // but that one doesn't work because I can't get it to work recursively, see HACK id=0
            var original_args = args;
            if(args != null)
                for (int i = 0; i < args.Length; i++)
                    if (args[i] == null)
                        throw new ArgumentNullException($"args[{i}]. Use JSString.Null or JSString.Undefined instead.");

            args = args?.Map(arg => arg as JSString ?? new JSString(JsonSerializer.Serialize(arg, this.Options)));
            var identifierObj = new JSString(identifier);

            return await ProcessExtensions.ExecuteJS(this.Imports,
                                                     identifier: identifierObj,
                                                     arguments: args,
                                                     jsIdentifiers: this.IDs,
                                                     options: this.Options,
                                                     typeIdPropertyName: nameof(IJSSerializable.SERIALIZATION_TYPE_ID))
                                          .ConfigureAwait(false);
        }
        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object[]? args)
        {

            var (exitCode, stdOut, stdErr) = await this.InvokeAsyncImpl(identifier, args);
            Console.WriteLine(stdOut);
            Console.WriteLine(stdErr);
            if (stdOut == "")
                return default!;
            try
            {
                Contract.Assert(!stdOut.Contains(nameof(IJSSerializable.SERIALIZATION_TYPE_ID)));
                return JsonSerializer.Deserialize<TValue>(stdOut, this.Options);
            }
            catch (JsonException)
            {
                Console.WriteLine(stdOut);
                throw;
            }
        }

        ValueTask<TValue> IJSRuntime.InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args)
        {
            return this.InvokeAsync<TValue>(identifier, args);
        }
    }
}