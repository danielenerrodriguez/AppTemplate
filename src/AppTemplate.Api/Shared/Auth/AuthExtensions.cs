using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Api.Shared.Auth;

/// <summary>
/// ASP.NET Identity scaffolding — pre-wired for future use. Registers Identity endpoints
/// at /api/auth, AppDbContext with SQLite, and authorization services. No endpoints are
/// currently protected; add [Authorize] when you need authentication on a feature.
/// </summary>
public static class AuthExtensions
{
    public static IServiceCollection AddAppAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=AppTemplate.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddIdentityApiEndpoints<IdentityUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddAuthorizationBuilder();

        return services;
    }

    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/auth")
            .MapIdentityApi<IdentityUser>()
            .WithTags("Auth");

        return app;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
