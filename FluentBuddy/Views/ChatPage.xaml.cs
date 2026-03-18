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

        try
        {
            SendButton.IsEnabled = false;
            SendButton.Text = "Sending...";           
            ResponseLabel.Text = "Generating response...";

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
        catch (Exception ex)
        {            
            ResponseLabel.Text = $"Something went wrong: {ex.Message}";
        }
        finally
        {
            SendButton.IsEnabled = true;
            SendButton.Text = "Send Message";
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//settings");
    }
}