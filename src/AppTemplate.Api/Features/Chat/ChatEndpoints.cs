using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace AppTemplate.Api.Features.Chat;

public static class ChatEndpoints
{
    public static RouteGroupBuilder MapChatEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (ChatRequestDto request, IChatService chatService, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Results.BadRequest("Message is required.");
            }

            var response = await chatService.SendMessageAsync(request, ct);
            return TypedResults.Ok(response);
        }).WithName("SendChatMessage");

        group.MapPost("/stream", async (ChatRequestDto request, IChatService chatService, HttpContext context, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Message is required.", ct);
                return;
            }

            context.Response.ContentType = "text/event-stream";
            context.Response.Headers.CacheControl = "no-cache";
            context.Response.Headers.Connection = "keep-alive";

            await foreach (var chunk in chatService.StreamMessageAsync(request, ct))
            {
                await context.Response.WriteAsync($"data: {chunk}\n\n", Encoding.UTF8, ct);
                await context.Response.Body.FlushAsync(ct);
            }

            await context.Response.WriteAsync("data: [DONE]\n\n", Encoding.UTF8, ct);
            await context.Response.Body.FlushAsync(ct);
        }).WithName("StreamChatMessage");

        group.MapGet("/history/{deviceId}", async (string deviceId, IChatService chatService, CancellationToken ct) =>
        {
            var history = await chatService.GetChatHistoryAsync(deviceId, ct);
            return TypedResults.Ok(history);
        }).WithName("GetChatHistory");

        group.MapDelete("/history/{deviceId}", async (string deviceId, IChatService chatService, CancellationToken ct) =>
        {
            await chatService.ClearChatHistoryAsync(deviceId, ct);
            return TypedResults.Ok();
        }).WithName("ClearChatHistory");

        group.MapGet("/models", async ([FromQuery] string? deviceId, IChatService chatService, IConfiguration config, ILoggerFactory loggerFactory, CancellationToken ct) =>
        {
            var defaultModel = config.GetValue<string>("Anthropic:Model") ?? "claude-sonnet-4-20250514";
            try
            {
                var models = await chatService.ListModelsAsync(deviceId, ct);
                return Results.Ok(new { Models = models, DefaultModel = defaultModel });
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger("ChatEndpoints");
                logger.LogWarning(ex, "Failed to list models for device {DeviceId}", deviceId);
                return Results.Ok(new { Models = Array.Empty<ModelListDto>(), DefaultModel = defaultModel });
            }
        }).WithName("ListModels");

        group.MapGet("/env-key-available", (IConfiguration config) =>
        {
            var envKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")
                ?? config.GetValue<string>("Anthropic:ApiKey");

            return TypedResults.Ok(new { Available = !string.IsNullOrEmpty(envKey) });
        }).WithName("CheckEnvKeyAvailable");

        return group;
    }
}
