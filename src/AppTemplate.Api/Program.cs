using AppTemplate.Api.Features.ApiKeys;
using AppTemplate.Api.Features.Chat;
using AppTemplate.Api.Features.Weather;
using AppTemplate.Api.Shared.Auth;
using AppTemplate.Api.Shared.Extensions;
using AppTemplate.Api.Shared.Logging;
using AppTemplate.Api.Shared.Middleware;
using Serilog;

const string CorsPolicyName = "BlazorClient";

SerilogConfig.Configure();

try
{
    Log.Information("Starting AppTemplate API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Add OpenAPI
    builder.Services.AddOpenApi();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(CorsPolicyName, policy =>
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? ["http://localhost:8080"];

            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // Add feature services
    builder.Services.AddWeatherFeature();
    builder.Services.AddChatFeature();
    builder.Services.AddAIServices();
    builder.Services.AddSecurityServices();

    // Add authentication & authorization
    builder.Services.AddAppAuthentication(builder.Configuration);

    var app = builder.Build();

    // Initialize database (ensure created)
    await app.InitializeDatabaseAsync();

    // Configure the HTTP request pipeline
    app.UseErrorHandling();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseCors(CorsPolicyName);

    app.UseAuthentication();
    app.UseAuthorization();

    // Map feature endpoints
    app.MapGroup("/api/weather")
        .MapWeatherEndpoints()
        .WithTags("Weather");

    app.MapGroup("/api/chat")
        .MapChatEndpoints()
        .WithTags("Chat");

    app.MapGroup("/api/apikeys")
        .MapApiKeyEndpoints()
        .WithTags("ApiKeys");

    // Map auth endpoints
    app.MapAuthEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
