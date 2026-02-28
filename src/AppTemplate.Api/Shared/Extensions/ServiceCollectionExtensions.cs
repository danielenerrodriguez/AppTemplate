using AppTemplate.Api.Features.Chat;
using AppTemplate.Api.Features.Weather;
using AppTemplate.Api.Shared.AI;
using AppTemplate.Api.Shared.Security;

namespace AppTemplate.Api.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherFeature(this IServiceCollection services)
    {
        services.AddScoped<IWeatherService, WeatherService>();
        return services;
    }

    public static IServiceCollection AddChatFeature(this IServiceCollection services)
    {
        services.AddScoped<IChatService, ChatService>();
        return services;
    }

    public static IServiceCollection AddAIServices(this IServiceCollection services)
    {
        // Singleton: ClaudeService holds config + a default AnthropicClient.
        // Per-user-key clients are created on-demand within each method call.
        services.AddSingleton<IAIService, ClaudeService>();
        return services;
    }

    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddDataProtection();
        services.AddSingleton<IApiKeyProtector, ApiKeyProtector>();
        return services;
    }
}
