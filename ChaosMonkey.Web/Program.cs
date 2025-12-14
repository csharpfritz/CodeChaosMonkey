using ChaosMonkey.Web.Endpoints;
using ChaosMonkey.Web.Services;
using ChaosMonkey.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services
builder.Services.AddSignalR();
builder.Services.AddSingleton<GitHubService>();
builder.Services.AddSingleton<JsonQueueService>();
builder.Services.AddSingleton<ChaosCommandExecutor>();
builder.Services.AddHostedService<ChaosExecutorService>();

var app = builder.Build();

// Configure the HTTP request pipeline

app.UseHttpsRedirection();

// Serve static files for the overlay
app.UseStaticFiles();

// Map webhook endpoints
app.MapTiltifyEndpoints();

// Map queue endpoints
app.MapQueueEndpoints();

// Map SignalR hub
app.MapHub<ChaosStatusHub>("/hubs/chaos");

app.MapDefaultEndpoints();

// Add a simple root endpoint
app.MapGet("/", () => new { 
    Service = "Chaos Monkey Web API", 
    Status = "Running",
    Endpoints = new[] { "/webhooks/tiltify", "/webhooks/tiltify/health", "/queue/status" }
}).WithName("Root").WithTags("Info");

app.Run();
