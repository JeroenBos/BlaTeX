using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text.Json;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("app");

builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton(serviceType: typeof(IKaTeXRuntime), implementationType: typeof(KaTeXRuntime));

var host = builder.Build();

GetIJSRuntimeServiceJsonSerializerOptions(host.Services).AddKaTeXJsonConverters();

await host.RunAsync();





static JsonSerializerOptions GetIJSRuntimeServiceJsonSerializerOptions(IServiceProvider services)
{
    var jsRuntime = services.GetService<IJSRuntime>();
    var prop = typeof(JSRuntime).GetProperty("JsonSerializerOptions", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("Reflection configuration error");
    return (JsonSerializerOptions)prop.GetValue(jsRuntime, null)!;
}
