using FluentBuddy.Services;

namespace FluentBuddy.Views;

public partial class DictionaryPage : ContentPage
{
    private readonly SettingsService _settingsService;

    public DictionaryPage()
    {
        InitializeComponent();
        _settingsService = new SettingsService();
    }

    private async void OnAnalyzeClicked(object sender, EventArgs e)
    {
        var word = WordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(word))
        {           
            MeaningLabel.Text = "Please enter a word.";
            return;
        }

        try
        {
            AnalyzeButton.IsEnabled = false;
            AnalyzeButton.Text = "Analyzing...";            

            ResetResultState();
            WordLabel.Text = word;
            MeaningLabel.Text = "Generating analysis...";

            var settings = _settingsService.GetSettings();

            IAiService aiService = settings.Provider == "Gemini"
                ? new GeminiService()
                : new OpenAiService();

            var result = await aiService.AnalyzeWordAsync(
                word,
                settings.EnglishLevel,
                settings.ApiKey);

            WordLabel.Text = result.Word;
            MeaningLabel.Text = result.Meaning;
            PartOfSpeechLabel.Text = result.PartOfSpeech;
            FormsLabel.Text = JoinList(result.Forms);
            WordFamilyLabel.Text = JoinList(result.WordFamily);
            SynonymsLabel.Text = JoinList(result.Synonyms);
            AntonymsLabel.Text = JoinList(result.Antonyms);
            ExamplesLabel.Text = JoinLines(result.Examples);
            UsageLabel.Text = result.Usage;           
        }
        catch (Exception ex)
        {            
            WordLabel.Text = "Analysis failed";
            MeaningLabel.Text = $"Something went wrong: {ex.Message}";
            PartOfSpeechLabel.Text = "—";
            FormsLabel.Text = "—";
            WordFamilyLabel.Text = "—";
            SynonymsLabel.Text = "—";
            AntonymsLabel.Text = "—";
            ExamplesLabel.Text = "—";
            UsageLabel.Text = "—";
        }
        finally
        {
            AnalyzeButton.IsEnabled = true;
            AnalyzeButton.Text = "Analyze Word";
        }
    }

    private void ResetResultState()
    {
        MeaningLabel.Text = "—";
        PartOfSpeechLabel.Text = "—";
        FormsLabel.Text = "—";
        WordFamilyLabel.Text = "—";
        SynonymsLabel.Text = "—";
        AntonymsLabel.Text = "—";
        ExamplesLabel.Text = "—";
        UsageLabel.Text = "—";
    }

    private static string JoinList(List<string>? items)
    {
        return items is { Count: > 0 }
            ? string.Join(", ", items)
            : "—";
    }

    private static string JoinLines(List<string>? items)
    {
        return items is { Count: > 0 }
            ? string.Join(Environment.NewLine, items.Select(x => $"• {x}"))
            : "—";
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