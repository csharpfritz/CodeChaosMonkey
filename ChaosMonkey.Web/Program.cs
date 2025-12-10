using ChaosMonkey.Web.Endpoints;
using ChaosMonkey.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services
builder.Services.AddSingleton<GitHubService>();

var app = builder.Build();

// Configure the HTTP request pipeline

app.UseHttpsRedirection();

// Map webhook endpoints
app.MapTiltifyEndpoints();

app.MapDefaultEndpoints();

// Add a simple root endpoint
app.MapGet("/", () => new { 
    Service = "Chaos Monkey Web API", 
    Status = "Running",
    Endpoints = new[] { "/webhooks/tiltify", "/webhooks/tiltify/health" }
}).WithName("Root").WithTags("Info");

app.Run();
