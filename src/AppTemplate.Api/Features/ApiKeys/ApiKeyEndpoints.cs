using AppTemplate.Api.Shared.Auth;
using AppTemplate.Api.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Api.Features.ApiKeys;

public static class ApiKeyEndpoints
{
    public static RouteGroupBuilder MapApiKeyEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (ApiKeyRequestDto request, AppDbContext db, IApiKeyProtector protector, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.DeviceId) || string.IsNullOrWhiteSpace(request.ApiKey))
            {
                return Results.BadRequest("DeviceId and ApiKey are required.");
            }

            var encryptedKey = protector.Protect(request.ApiKey);

            var existing = await db.ApiKeys
                .FirstOrDefaultAsync(k => k.DeviceId == request.DeviceId, ct);

            if (existing is not null)
            {
                existing.ApiKey = encryptedKey;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                db.ApiKeys.Add(new Shared.Data.ApiKeyEntity
                {
                    DeviceId = request.DeviceId,
                    ApiKey = encryptedKey,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync(ct);

            // Mask from the original plain-text key (not the encrypted one)
            return TypedResults.Ok(
                new ApiKeyStatusDto(HasKey: true, MaskedKey: MaskKey(request.ApiKey)));
        }).WithName("SaveApiKey");

        group.MapGet("/{deviceId}", async (string deviceId, AppDbContext db, IApiKeyProtector protector, CancellationToken ct) =>
        {
            var entity = await db.ApiKeys
                .FirstOrDefaultAsync(k => k.DeviceId == deviceId, ct);

            if (entity is null)
            {
                return TypedResults.Ok(new ApiKeyStatusDto(HasKey: false));
            }

            var plainKey = protector.Unprotect(entity.ApiKey);
            return TypedResults.Ok(
                new ApiKeyStatusDto(HasKey: true, MaskedKey: MaskKey(plainKey)));
        }).WithName("GetApiKeyStatus");

        group.MapDelete("/{deviceId}", async (string deviceId, AppDbContext db, CancellationToken ct) =>
        {
            var entity = await db.ApiKeys
                .FirstOrDefaultAsync(k => k.DeviceId == deviceId, ct);

            if (entity is not null)
            {
                db.ApiKeys.Remove(entity);
                await db.SaveChangesAsync(ct);
            }

            return TypedResults.Ok(new ApiKeyStatusDto(HasKey: false));
        }).WithName("DeleteApiKey");

        return group;
    }

    internal static string MaskKey(string apiKey)
    {
        if (apiKey.Length <= 8)
        {
            return "sk-ant-****";
        }

        var last4 = apiKey[^4..];
        return $"sk-ant-****{last4}";
    }
}
