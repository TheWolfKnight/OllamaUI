using OllamaUi.Blazor.Components;
using OllamaUi.Blazor.Models;
using OllamaUi.Caller.Interfaces;
using OllamaUi.Caller.Services;
using Serilog;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLogging(loggingBuilder => {
  LoggerConfiguration config = new LoggerConfiguration()
    .WriteTo.Console();

  loggingBuilder.AddSerilog(config.CreateLogger());
});

builder.Services.AddScoped<IOllamaModelCaller, OllamaModelCaller>();
builder.Services.AddScoped<IOllamaGenerationCaller, OllamaGenerationCaller>();

var homeDirectory = Environment.GetEnvironmentVariable("HOME", EnvironmentVariableTarget.Process) ?? "";

builder.Services.AddSingleton<PageState>(new PageState { ChatSaveLocation = Path.Combine(homeDirectory, ".ollama-ui-chats") });

builder.Services.AddRadzenComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
