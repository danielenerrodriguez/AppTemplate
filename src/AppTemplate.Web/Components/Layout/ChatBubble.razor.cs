using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;

namespace AppTemplate.Web.Components.Layout;

/// <summary>
/// Global floating AI chat bubble — rendered in MainLayout so it appears on every page.
/// This is a layout-level component, not a feature page. It manages its own state:
/// API key setup, chat messages, streaming, model selection, and per-device history.
/// </summary>
public partial class ChatBubble : ComponentBase, IDisposable
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILogger<ChatBubble> Logger { get; set; } = default!;

    private readonly CancellationTokenSource _cts = new();

    private bool _isOpen;
    private ChatView _currentView = ChatView.Setup;
    private string? _deviceId;

    // Setup state
    private string _apiKeyInput = string.Empty;
    private string? _setupError;
    private bool _envKeyAvailable;

    // Chat state
    private readonly List<BubbleChatMessage> _messages = [];
    private string _userInput = string.Empty;
    private bool _isLoading;
    private string? _chatError;
    private ElementReference _messagesRef;

    // Settings state
    private string? _maskedKey;
    private bool _usingEnvKey;
    private bool _confirmingDelete;

    // Model state
    private List<ModelOption> _availableModels = [];
    private string? _selectedModel;
    private string? _defaultModel;
    private bool _loadingModels;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _deviceId = await JS.InvokeAsync<string>("chatDevice.getDeviceId", _cts.Token);
                _selectedModel = await JS.InvokeAsync<string>("chatDevice.getModel", _cts.Token);
                if (string.IsNullOrEmpty(_selectedModel)) _selectedModel = null;
                await CheckInitialState();
                StateHasChanged();
            }
            catch (OperationCanceledException)
            {
                // Component disposed during initialization — expected
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected — expected during navigation
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "JS interop not ready on first render — will retry on open");
            }
        }
    }

    private async Task CheckInitialState()
    {
        if (string.IsNullOrEmpty(_deviceId)) return;

        try
        {
            var ct = _cts.Token;

            // Check env key availability
            var envResponse = await Http.GetFromJsonAsync<EnvKeyResponse>("api/chat/env-key-available", ct);
            _envKeyAvailable = envResponse?.Available ?? false;

            // Check if user has a stored key
            var keyResponse = await Http.GetFromJsonAsync<ApiKeyStatusResponse>(
                $"api/apikeys/{_deviceId}", ct);

            if (keyResponse?.HasKey == true)
            {
                _maskedKey = keyResponse.MaskedKey;
                _currentView = ChatView.Chat;
                await LoadChatHistory();
            }
            else if (_envKeyAvailable)
            {
                _usingEnvKey = true;
                _currentView = ChatView.Chat;
                await LoadChatHistory();
            }
        }
        catch (OperationCanceledException)
        {
            // Component disposed — expected
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "API not reachable during initial state check — staying on setup");
        }
    }

    private async Task ToggleChat()
    {
        _isOpen = !_isOpen;

        if (_isOpen && string.IsNullOrEmpty(_deviceId))
        {
            try
            {
                _deviceId = await JS.InvokeAsync<string>("chatDevice.getDeviceId", _cts.Token);
                _selectedModel = await JS.InvokeAsync<string>("chatDevice.getModel", _cts.Token);
                if (string.IsNullOrEmpty(_selectedModel)) _selectedModel = null;
                await CheckInitialState();
            }
            catch (OperationCanceledException)
            {
                // Component disposed — expected
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected — expected
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "JS interop not ready on toggle — will retry");
            }
        }
    }

    private async Task UseEnvKey()
    {
        _usingEnvKey = true;
        _currentView = ChatView.Chat;
        await LoadChatHistory();
    }

    private async Task SaveApiKey()
    {
        if (string.IsNullOrWhiteSpace(_apiKeyInput) || string.IsNullOrEmpty(_deviceId))
            return;

        _setupError = null;

        try
        {
            var request = new { DeviceId = _deviceId, ApiKey = _apiKeyInput };
            var response = await Http.PostAsJsonAsync("api/apikeys", request, _cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiKeyStatusResponse>(_cts.Token);
                _maskedKey = result?.MaskedKey;
                _apiKeyInput = string.Empty;
                _currentView = ChatView.Chat;
                await LoadChatHistory();
            }
            else
            {
                _setupError = "Failed to save API key. Please try again.";
            }
        }
        catch (OperationCanceledException)
        {
            // Component disposed — expected
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to save API key");
            _setupError = $"Error: {ex.Message}";
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_userInput) || _isLoading) return;

        var userMessage = _userInput.Trim();
        _userInput = string.Empty;
        _chatError = null;

        _messages.Add(new BubbleChatMessage(userMessage, IsUser: true));
        var aiMessage = new BubbleChatMessage(string.Empty, IsUser: false, IsStreaming: true);
        _messages.Add(aiMessage);
        _isLoading = true;

        try
        {
            var request = new { Message = userMessage, DeviceId = _deviceId, Model = _selectedModel };
            var response = await Http.PostAsJsonAsync("api/chat", request, _cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(_cts.Token);
                throw new HttpRequestException($"API returned {response.StatusCode}: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<ChatResponseResult>(_cts.Token);
            aiMessage = aiMessage with { Content = result?.Response ?? "No response", IsStreaming = false };
            _messages[^1] = aiMessage;
        }
        catch (OperationCanceledException)
        {
            _messages.Remove(aiMessage);
            // Component disposed mid-request — expected
        }
        catch (Exception ex)
        {
            _messages.Remove(aiMessage);
            Logger.LogWarning(ex, "Failed to send chat message");
            _chatError = $"Failed: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessage();
        }
    }

    private async Task LoadChatHistory()
    {
        if (string.IsNullOrEmpty(_deviceId)) return;

        try
        {
            var history = await Http.GetFromJsonAsync<List<ChatHistoryItem>>(
                $"api/chat/history/{_deviceId}", _cts.Token);

            if (history is not null && history.Count > 0)
            {
                _messages.Clear();
                foreach (var item in history)
                {
                    _messages.Add(new BubbleChatMessage(item.Content, item.IsUser));
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Component disposed — expected
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load chat history for device {DeviceId}", _deviceId);
        }
    }

    private async Task LoadModels()
    {
        if (_availableModels.Count > 0) return; // Already loaded

        _loadingModels = true;
        try
        {
            var deviceParam = _deviceId is not null ? $"?deviceId={_deviceId}" : "";
            var result = await Http.GetFromJsonAsync<ModelsResponse>($"api/chat/models{deviceParam}", _cts.Token);

            if (result?.Models is not null)
            {
                _availableModels = result.Models
                    .Select(m => new ModelOption(m.Id, m.DisplayName))
                    .ToList();
                _defaultModel = result.DefaultModel;

                if (_selectedModel is null)
                {
                    _selectedModel = _defaultModel;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Component disposed — expected
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to load models");
        }
        finally
        {
            _loadingModels = false;
        }
    }

    private async Task OnModelChanged(string value)
    {
        _selectedModel = value;
        try
        {
            await JS.InvokeVoidAsync("chatDevice.setModel", _cts.Token, value);
        }
        catch (OperationCanceledException)
        {
            // Component disposed — expected
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected — expected
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Failed to persist model selection to localStorage");
        }
    }

    private async Task OpenSettings()
    {
        _currentView = ChatView.Settings;
        await LoadModels();
    }

    private Task ConfirmDeleteKey()
    {
        _confirmingDelete = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task ConfirmDeleteYes()
    {
        _confirmingDelete = false;
        await DeleteApiKey();
    }

    private void ConfirmDeleteNo()
    {
        _confirmingDelete = false;
    }

    private async Task DeleteApiKey()
    {
        if (string.IsNullOrEmpty(_deviceId)) return;

        try
        {
            await Http.DeleteAsync($"api/apikeys/{_deviceId}", _cts.Token);
            _maskedKey = null;

            if (_envKeyAvailable)
            {
                _usingEnvKey = true;
            }
            else
            {
                _currentView = ChatView.Setup;
            }
        }
        catch (OperationCanceledException)
        {
            // Component disposed — expected
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to delete API key for device {DeviceId}", _deviceId);
        }
    }

    private async Task ClearHistory()
    {
        if (string.IsNullOrEmpty(_deviceId)) return;

        try
        {
            await Http.DeleteAsync($"api/chat/history/{_deviceId}", _cts.Token);
            _messages.Clear();
            _currentView = ChatView.Chat;
        }
        catch (OperationCanceledException)
        {
            // Component disposed — expected
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to clear chat history for device {DeviceId}", _deviceId);
        }
    }

    private static string GetMessageStyle(bool isUser) =>
        isUser
            ? "background-color: var(--mud-palette-primary); color: white; border-radius: 16px 16px 4px 16px; max-width: 85%;"
            : "background-color: var(--mud-palette-surface); border-radius: 16px 16px 16px 4px; max-width: 85%;";

    private string GetModelDisplayName()
    {
        if (_selectedModel is null) return "Default";
        var model = _availableModels.FirstOrDefault(m => m.Id == _selectedModel);
        return model?.DisplayName ?? _selectedModel;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    // Internal types
    private enum ChatView { Setup, Chat, Settings }

    private record BubbleChatMessage(
        string Content,
        bool IsUser,
        bool IsStreaming = false,
        DateTime Timestamp = default)
    {
        public DateTime Timestamp { get; init; } = Timestamp == default ? DateTime.UtcNow : Timestamp;
    }

    private record ModelOption(string Id, string DisplayName);

    // JSON deserialization types
    private record EnvKeyResponse(bool Available);
    private record ApiKeyStatusResponse(bool HasKey, string? MaskedKey);
    private record ChatResponseResult(string Response, DateTime Timestamp);
    private record ChatHistoryItem(string Content, bool IsUser, DateTime Timestamp);
    private record ModelsResponse(List<ModelItem> Models, string DefaultModel);
    private record ModelItem(string Id, string DisplayName);
}
