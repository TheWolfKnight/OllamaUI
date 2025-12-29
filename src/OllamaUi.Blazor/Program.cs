using System;
using System.Net.Http;
using OllamaUi.Blazor;
using OllamaUi.Caller.Interfaces;
using OllamaUi.Caller.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLogging(loggingBuilder => {
  LoggerConfiguration config = new LoggerConfiguration()
    .WriteTo.Console();

  loggingBuilder.AddSerilog(config.CreateLogger());
});

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IOllamaModelCaller, OllamaModelCaller>();
builder.Services.AddScoped<IOllamaGenerationCaller, OllamaGenerationCaller>();

await builder.Build().RunAsync();
