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

namespace BlaTeX.Tests
{
    class NodeJSRuntime : IJSRuntime
    {
        public JsonSerializerOptions Options { get; }
        public IReadOnlyList<string> Imports { get; }
        public NodeJSRuntime(IEnumerable<string> imports, JsonSerializerOptions? options = null)
        {
            this.Imports = imports?.ToReadOnlyList() ?? EmptyCollection<string>.ReadOnlyList;
            this.Options = options ?? new JsonSerializerOptions();
        }

        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, object[]? args)
        {
            var (exitCode, stdOut, stdErr) = await ProcessExtensions.ExecuteJS(this.Imports, identifier, args, this.Options).ConfigureAwait(false);
            Console.WriteLine(stdErr);
            if (stdOut == "")
                return default!;
            return JsonSerializer.Deserialize<TValue>(stdOut, this.Options);
        }

        ValueTask<TValue> IJSRuntime.InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object[] args)
        {
            return this.InvokeAsync<TValue>(identifier, args);
        }
    }
}