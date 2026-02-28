using AppTemplate.Api.Shared.AI;
using AppTemplate.Api.Shared.Auth;
using AppTemplate.Api.Shared.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AppTemplate.Api.Tests;

/// <summary>
/// Test server factory that swaps SQLite for in-memory EF Core and mocks the AI service.
/// Removes ALL EF/Identity-related descriptors then re-adds them with InMemory to avoid
/// the "multiple database providers registered" conflict between SQLite and InMemory.
/// Usage: var factory = new CustomWebApplicationFactory(); var client = factory.CreateClient();
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb-{Guid.NewGuid()}";
    public IAIService MockAiService { get; } = Substitute.For<IAIService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Provide a fake API key so ChatService.ResolveApiKeyAsync succeeds
        builder.UseSetting("Anthropic:ApiKey", "sk-test-integration-key");

        builder.ConfigureServices(services =>
        {
            // Remove ALL EF + Identity store descriptors to get a clean slate.
            // The SQLite provider registers internal services that conflict with
            // InMemory. Identity's AddEntityFrameworkStores also registers EF-based
            // IUserStore/IRoleStore that depend on the SQLite provider, so those
            // must be removed and re-added too.
            var descriptorsToRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                    || d.ServiceType == typeof(DbContextOptions)
                    || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
                    || d.ImplementationType?.FullName?.Contains("EntityFrameworkCore") == true
                    || d.ServiceType.FullName?.Contains("IUserStore") == true
                    || d.ServiceType.FullName?.Contains("IRoleStore") == true)
                .ToList();
            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            // Re-add DbContext with InMemory provider
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Re-add Identity EF stores so IUserStore<IdentityUser> resolves
            services.AddIdentityCore<IdentityUser>()
                .AddEntityFrameworkStores<AppDbContext>();

            // Replace IAIService with mock
            var aiDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAIService));
            if (aiDescriptor is not null)
                services.Remove(aiDescriptor);
            services.AddSingleton(MockAiService);

            // Replace IApiKeyProtector with a passthrough (no real encryption in tests)
            var protectorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IApiKeyProtector));
            if (protectorDescriptor is not null)
                services.Remove(protectorDescriptor);

            var mockProtector = Substitute.For<IApiKeyProtector>();
            mockProtector.Protect(Arg.Any<string>()).Returns(c => c.Arg<string>());
            mockProtector.Unprotect(Arg.Any<string>()).Returns(c => c.Arg<string>());
            services.AddSingleton(mockProtector);
        });
    }
}
