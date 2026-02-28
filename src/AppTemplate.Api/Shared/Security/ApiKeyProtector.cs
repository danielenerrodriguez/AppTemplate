using Microsoft.AspNetCore.DataProtection;

namespace AppTemplate.Api.Shared.Security;

/// <summary>
/// Encrypts and decrypts API keys at rest using ASP.NET Data Protection.
/// Keys are encrypted before storing in SQLite and decrypted when read.
/// </summary>
public interface IApiKeyProtector
{
    string Protect(string plainTextKey);
    string Unprotect(string protectedKey);
}

public class ApiKeyProtector : IApiKeyProtector
{
    private readonly IDataProtector _protector;

    public ApiKeyProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("AppTemplate.ApiKeys.v1");
    }

    public string Protect(string plainTextKey) => _protector.Protect(plainTextKey);
    public string Unprotect(string protectedKey) => _protector.Unprotect(protectedKey);
}
