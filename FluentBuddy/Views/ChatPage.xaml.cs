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
            StatusLabel.Text = "Missing input";
            StatusLabel.TextColor = Color.FromArgb("#FCA5A5");
            ResponseLabel.Text = "Please enter a message first.";
            return;
        }

        try
        {
            SendButton.IsEnabled = false;
            SendButton.Text = "Sending...";
            StatusLabel.Text = "Thinking...";
            StatusLabel.TextColor = Color.FromArgb("#FCD34D");
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
            StatusLabel.Text = "Done";
            StatusLabel.TextColor = Color.FromArgb("#86EFAC");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Error";
            StatusLabel.TextColor = Color.FromArgb("#FCA5A5");
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