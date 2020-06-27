using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlaTeX.JSInterop.KaTeX;
using Microsoft.JSInterop;

namespace BlaTeX.JSInterop.KaTeX
{
    public interface IKaTeX
    {
        public static IKaTeX Create(IJSRuntime jsRuntime)
        {
            if (jsRuntime == null)
                throw new ArgumentNullException(nameof(jsRuntime));

            return new _KaTeX(jsRuntime);
        }
        Task<string> RenderToString(string math);
        Task<HtmlDomNode> RenderToDom(string math);
    }

    internal class _KaTeX : IKaTeX
    {
        private readonly IJSRuntime jsRuntime;

        public _KaTeX(IJSRuntime jsRuntime)
        {
            if (jsRuntime == null)
                throw new ArgumentNullException(nameof(jsRuntime));

            this.jsRuntime = jsRuntime;
        }

        public Task<string> RenderToString(string math) => InvokeAsync<string>("renderToString", math);
        public Task<HtmlDomNode> RenderToDom(string math) => InvokeAsync<HtmlDomNode>("__renderToDomTree", math);


        private Task<T> InvokeAsync<T>(string name, params object[] arguments)
        {
            if (string.IsNullOrWhiteSpace(name))
                if (name == null)
                    throw new ArgumentNullException(nameof(jsRuntime));
                else
                    throw new ArgumentException("String empty", nameof(name));
            if (arguments == null)
                throw new ArgumentNullException(nameof(jsRuntime));

            return jsRuntime.InvokeAsync<T>("katex." + name, arguments)
                            .AsTask();
        }
    }

}