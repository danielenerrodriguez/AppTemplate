using AppTemplate.Web.Components.Layout;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using NSubstitute;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AppTemplate.Web.Tests.Layout;

/// <summary>
/// bUnit tests for the ChatBubble component. Uses IAsyncLifetime instead of
/// inheriting BunitContext directly, because MudBlazor's PopoverService only
/// implements IAsyncDisposable and bUnit's synchronous Dispose() throws.
/// </summary>
public class ChatBubbleTests : IAsyncLifetime
{
    private readonly BunitContext _ctx = new();
    private readonly IJSRuntime _mockJs;
    private readonly HttpClient _mockHttp;

    public ChatBubbleTests()
    {
        _ctx.Services.AddMudServices();

        _mockJs = Substitute.For<IJSRuntime>();
        _ctx.Services.AddSingleton(_mockJs);

        // HttpClient with mock handler that returns default responses
        _mockHttp = CreateMockHttpClient(request =>
        {
            var path = request.RequestUri?.PathAndQuery ?? "";

            if (path.Contains("env-key-available"))
            {
                return JsonResponse(new { Available = false });
            }
            if (path.Contains("apikeys"))
            {
                return JsonResponse(new { HasKey = false, MaskedKey = (string?)null });
            }
            if (path.Contains("history"))
            {
                return JsonResponse(Array.Empty<object>());
            }
            if (path.Contains("models"))
            {
                return JsonResponse(new { Models = Array.Empty<object>(), DefaultModel = "claude-sonnet-4-20250514" });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        _ctx.Services.AddSingleton(_mockHttp);
        _ctx.Services.AddSingleton(Substitute.For<ILogger<ChatBubble>>());
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        _mockHttp.Dispose();
        await _ctx.DisposeAsync();
    }

    [Fact]
    public void ChatBubble_InitialRender_ShowsFabButton()
    {
        var cut = _ctx.Render<ChatBubble>();

        // The chat FAB button should be visible (chat is closed by default)
        cut.Markup.Should().Contain("chat-bubble-container");
        cut.Markup.Should().Contain("mud-fab");
    }

    [Fact]
    public void ChatBubble_InitialRender_ChatPanelIsHidden()
    {
        var cut = _ctx.Render<ChatBubble>();

        // The full chat panel (MudPaper with AI Chat header) should NOT be visible
        cut.Markup.Should().NotContain("AI Chat");
    }

    [Fact]
    public void ChatBubble_ClickFab_OpensChatPanel()
    {
        var cut = _ctx.Render<ChatBubble>();

        // Click the FAB to open
        var fab = cut.Find("button.mud-fab");
        fab.Click();

        // Now the chat panel should be visible with the header
        cut.Markup.Should().Contain("AI Chat");
    }

    [Fact]
    public void ChatBubble_OpenThenClose_HidesChatPanel()
    {
        var cut = _ctx.Render<ChatBubble>();

        // Open
        cut.Find("button.mud-fab").Click();
        cut.Markup.Should().Contain("AI Chat");

        // Close — find the MudIconButton with the Close icon in the toolbar.
        // The Close icon renders as an SVG path inside the button; look for
        // the second MudIconButton in the toolbar (first is Settings).
        var toolbarButtons = cut.FindAll(".mud-toolbar button.mud-icon-button");
        // The last icon button in the toolbar is the close (X) button
        var closeButton = toolbarButtons.Last();
        closeButton.Click();

        // Should be back to FAB only
        cut.Markup.Should().NotContain("AI Chat");
        cut.Markup.Should().Contain("mud-fab");
    }

    private static HttpClient CreateMockHttpClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    {
        var mockHandler = new MockHandler(handler);
        return new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost") };
    }

    private static Task<HttpResponseMessage> JsonResponse<T>(T body)
    {
        var json = JsonSerializer.Serialize(body);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }

    private class MockHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;
        public MockHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) => _handler = handler;
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) => _handler(request);
    }
}
