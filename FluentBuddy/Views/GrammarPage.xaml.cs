using FluentBuddy.Models;
using FluentBuddy.Services;

namespace FluentBuddy.Views;

public partial class GrammarPage : ContentPage
{
    private readonly SettingsService _settingsService;

    public GrammarPage()
    {
        InitializeComponent();
        _settingsService = new SettingsService();
    }

    private async void OnCorrectClicked(object sender, EventArgs e)
    {
        var sentence = SentenceEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(sentence))
        {
            CorrectedLabel.Text = "—";
            ExplanationLabel.Text = "Please enter a sentence.";
            ExampleLabel.Text = "—";
            return;
        }

        try
        {
            CorrectButton.IsEnabled = false;
            CorrectButton.Text = "Correcting...";

            CorrectedLabel.Text = "Generating correction...";
            ExplanationLabel.Text = "Analyzing grammar...";
            ExampleLabel.Text = "Preparing example...";

            var settings = _settingsService.GetSettings();

            IAiService aiService = settings.Provider == "Gemini"
                ? new GeminiService()
                : new OpenAiService();

            var result = await aiService.CorrectGrammarAsync(
                sentence,
                settings.EnglishLevel,
                settings.ApiKey);

            CorrectedLabel.Text = result.CorrectedSentence;
            ExplanationLabel.Text = result.Explanation;
            ExampleLabel.Text = result.ExtraExample;
        }
        catch (Exception ex)
        {
            CorrectedLabel.Text = "Unable to correct the sentence.";
            ExplanationLabel.Text = $"Something went wrong: {ex.Message}";
            ExampleLabel.Text = "—";
        }
        finally
        {
            CorrectButton.IsEnabled = true;
            CorrectButton.Text = "Correct Sentence";
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//settings");
    }

    private async void OnButtonPressed(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await button.ScaleTo(0.96, 80, Easing.CubicOut);
            await button.FadeTo(0.9, 80, Easing.CubicOut);
        }
    }

    private async void OnButtonReleased(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            await button.ScaleTo(1, 120, Easing.CubicOut);
            await button.FadeTo(1, 120, Easing.CubicOut);
        }
    }
}