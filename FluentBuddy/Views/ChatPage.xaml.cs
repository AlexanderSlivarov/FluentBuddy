using FluentBuddy.Services;

namespace FluentBuddy.Views;

public partial class ChatPage : ContentPage
{
    private readonly SettingsService _settingsService;

    public ChatPage()
    {
        InitializeComponent();
        _settingsService = new SettingsService();
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var userMessage = UserMessageEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            ResponseLabel.Text = "Please enter a message first.";
            return;
        }

        ResponseLabel.Text = "Loading...";

        var settings = _settingsService.GetSettings();

        IAiService aiService;

        if (settings.Provider == "Gemini")
        {
            aiService = new GeminiService();
        }
        else
        {
            aiService = new OpenAiService();
        }

        var response = await aiService.SendChatMessageAsync(
            userMessage,
            settings.EnglishLevel,
            settings.ApiKey);

        ResponseLabel.Text = response;
    }
}