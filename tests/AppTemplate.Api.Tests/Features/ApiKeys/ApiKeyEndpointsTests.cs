using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace AppTemplate.Api.Tests.Features.ApiKeys;

public class ApiKeyEndpointsTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApiKeyEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task PostApiKey_MissingDeviceId_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/apikeys", new { DeviceId = "", ApiKey = "sk-test" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostApiKey_MissingApiKey_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/apikeys", new { DeviceId = "dev-1", ApiKey = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostApiKey_ValidInput_ReturnsOkWithMaskedKey()
    {
        var response = await _client.PostAsJsonAsync("/api/apikeys",
            new { DeviceId = "device-post-1", ApiKey = "sk-ant-api03-abcdefghijkl" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiKeyStatusResult>();
        result!.HasKey.Should().BeTrue();
        result.MaskedKey.Should().StartWith("sk-ant-****");
        result.MaskedKey.Should().EndWith("jkl");
    }

    [Fact]
    public async Task PostApiKey_ExistingDevice_UpdatesKey()
    {
        // Create initial key
        await _client.PostAsJsonAsync("/api/apikeys",
            new { DeviceId = "device-update-1", ApiKey = "sk-ant-api03-original1234" });

        // Update key
        var response = await _client.PostAsJsonAsync("/api/apikeys",
            new { DeviceId = "device-update-1", ApiKey = "sk-ant-api03-updated99999" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiKeyStatusResult>();
        result!.MaskedKey.Should().EndWith("9999");
    }

    [Fact]
    public async Task GetApiKeyStatus_ExistingDevice_ReturnsHasKeyTrue()
    {
        await _client.PostAsJsonAsync("/api/apikeys",
            new { DeviceId = "device-get-1", ApiKey = "sk-ant-api03-gettest12345" });

        var response = await _client.GetAsync("/api/apikeys/device-get-1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiKeyStatusResult>();
        result!.HasKey.Should().BeTrue();
        result.MaskedKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetApiKeyStatus_UnknownDevice_ReturnsHasKeyFalse()
    {
        var response = await _client.GetAsync("/api/apikeys/nonexistent-device");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiKeyStatusResult>();
        result!.HasKey.Should().BeFalse();
        result.MaskedKey.Should().BeNull();
    }

    [Fact]
    public async Task DeleteApiKey_ExistingDevice_RemovesKeyAndReturnsHasKeyFalse()
    {
        await _client.PostAsJsonAsync("/api/apikeys",
            new { DeviceId = "device-del-1", ApiKey = "sk-ant-api03-todelete1234" });

        var response = await _client.DeleteAsync("/api/apikeys/device-del-1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiKeyStatusResult>();
        result!.HasKey.Should().BeFalse();

        // Verify it's actually gone
        var getResponse = await _client.GetAsync("/api/apikeys/device-del-1");
        var getResult = await getResponse.Content.ReadFromJsonAsync<ApiKeyStatusResult>();
        getResult!.HasKey.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteApiKey_NonexistentDevice_ReturnsOk()
    {
        var response = await _client.DeleteAsync("/api/apikeys/ghost-device");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostApiKey_ShortKey_MasksWithoutLastFour()
    {
        var response = await _client.PostAsJsonAsync("/api/apikeys",
            new { DeviceId = "device-short-1", ApiKey = "sk-tiny" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiKeyStatusResult>();
        result!.MaskedKey.Should().Be("sk-ant-****");
    }

    private record ApiKeyStatusResult(bool HasKey, string? MaskedKey);
}
