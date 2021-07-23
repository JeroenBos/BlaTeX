using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BlaTeX.JSInterop;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Reflection;
using BlaTeX.JSInterop.KaTeX;

namespace BlaTeX
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("app");

			builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
			builder.Services.AddSingleton(serviceType: typeof(IKaTeX), implementationType: typeof(_KaTeX));

			var host = builder.Build();

			ConfigureProviders(host.Services);

			await host.RunAsync();
		}

		public static void ConfigureProviders(IServiceProvider services)
		{
			GetIJSRuntimeServiceJsonSerializerOptions(services).AddKaTeXJsonConverters();
		}
		private static JsonSerializerOptions GetIJSRuntimeServiceJsonSerializerOptions(IServiceProvider services)
		{
			var jsRuntime = services.GetService<IJSRuntime>();
			var prop = typeof(JSRuntime).GetProperty("JsonSerializerOptions", BindingFlags.NonPublic | BindingFlags.Instance);
			return (JsonSerializerOptions)prop.GetValue(jsRuntime, null);
		}
	}
}
