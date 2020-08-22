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
        public NodeJSRuntime(IEnumerable<string> imports, JsonSerializerOptions? options = null)
        {
            this.Imports = imports?.ToReadOnlyList() ?? EmptyCollection<string>.ReadOnlyList;
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
            args = args?.Map(arg => arg as JSString ?? new JSString(JsonSerializer.Serialize(arg, this.Options)));
            var identifierObj = new JSString(identifier);

            return await ProcessExtensions.ExecuteJS(this.Imports, 
                                                     identifier: identifierObj,
                                                     arguments: args, 
                                                     options: this.Options)
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