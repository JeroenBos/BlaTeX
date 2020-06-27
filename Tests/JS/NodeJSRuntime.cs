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

namespace BlaTeX.Tests
{
    class NodeJSRuntime : IJSRuntime
    {
        public static NodeJSRuntime CreateDefault()
        {
            return new NodeJSRuntime($"{Program.RootFolder}/wwwroot/js/blatex.js".ToSingleton());
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

            }
            else
            {
                this.Options = options;
            }
        }

        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[]? args)
        {
            var (exitCode, stdOut, stdErr) = await ProcessExtensions.ExecuteJS(this.Imports, identifier, args, this.Options).ConfigureAwait(false);
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