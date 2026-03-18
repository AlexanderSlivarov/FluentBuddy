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
            ExplanationLabel.Text = "Please enter a sentence.";
            return;
        }

        var settings = _settingsService.GetSettings();

        IAiService aiService = settings.Provider == "Gemini"
            ? new GeminiService()
            : new OpenAiService();

        ExplanationLabel.Text = "Loading...";

        var result = await aiService.CorrectGrammarAsync(
            sentence,
            settings.EnglishLevel,
            settings.ApiKey);

        CorrectedLabel.Text = result.CorrectedSentence;
        ExplanationLabel.Text = result.Explanation;
        ExampleLabel.Text = result.ExtraExample;
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//settings");
    }
}