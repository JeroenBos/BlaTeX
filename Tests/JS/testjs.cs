using BlaTeX.JSInterop.KaTeX;
using JBSnorro;
using JBSnorro.Collections.ObjectModel;
using JBSnorro.Diagnostics;
using JBSnorro.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using static JBSnorro.Diagnostics.Contract;

namespace BlaTeX.Tests
{
    public class JSIntegrationTests
    {
        [Fact]
        public async Task JSInteropTestViaBlazor()
        {
            var imports = $"{Program.RootFolder}/wwwroot/js/blatex_wrapper.js".ToSingleton();
            var result = await new NodeJSRuntime(imports).InvokeAsync<string>("blatex_wrapper.default.renderToString", new[] { "c" }).AsTask();
            Assert(result == "<span class=\"katex\"><span class=\"katex-mathml\"><math><semantics><mrow><mi>c</mi></mrow><annotation encoding=\"application/x-tex\">c</annotation></semantics></math></span><span class=\"katex-html\" aria-hidden=\"true\"><span class=\"base\"><span class=\"strut\" style=\"height:0.43056em;vertical-align:0em;\"></span><span class=\"mord mathit\" data-loc=\"0,1\">c</span></span></span></span>");
        }

        [Fact]
        public void JsonDictionaryDeserializationUnderstanding()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DictionaryJsonConverter<object, ReadOnlyDictionary<string, object>>(_ => new ReadOnlyDictionary<string, object>(_)));
        }


    }
    public class AttributesDeserializationTests
    {
        private JsonSerializerOptions options = NodeJSRuntime.CreateDefault().Options;

        [Fact]
        public void EmptyAttributesDeserializationTest()
        {
            var attributes = JsonSerializer.Deserialize<Attributes>("{}", options);
            Contract.Assert(attributes.Count == 0);
        }
        [Fact]
        public void StringAttributesDeserializationTest()
        {
            var attributes = JsonSerializer.Deserialize<Attributes>("{\"a\":\"\"}", options);
            Contract.Assert(attributes.Count == 1);
            Contract.Assert(attributes.ContainsKey("a"));
            Contract.Assert("".Equals(attributes["a"]));
        }
        [Fact]
        public void SourceLocationAttributesDeserializationTest()
        {
            var attributes = JsonSerializer.Deserialize<Attributes>("{\"a\":\"\"}", options);
            Contract.Assert(attributes.Count == 1);
            Contract.Assert(attributes.ContainsKey("a"));
            Contract.Assert("".Equals(attributes["a"]));
        }
        [Fact]
        public void MemberAttributes()
        {
            var container = JsonSerializer.Deserialize<ContainsAttributes>("{ \"a\": { \"data-loc\": \"0,1\" }}", options);
            Contract.Assert(container?.a != null);
            Contract.Assert(container.a.SourceLocation != null);
            Contract.Assert(container.a.SourceLocation.Start == 0);
            Contract.Assert(container.a.SourceLocation.End == 1);
        }
        class ContainsAttributes
        {
            public Attributes a { get; set; }
        }
    }
}