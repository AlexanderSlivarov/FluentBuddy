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

        WordLabel.Text = "Loading...";
        MeaningLabel.Text = string.Empty;
        PartOfSpeechLabel.Text = string.Empty;
        FormsLabel.Text = string.Empty;
        WordFamilyLabel.Text = string.Empty;
        SynonymsLabel.Text = string.Empty;
        AntonymsLabel.Text = string.Empty;
        ExamplesLabel.Text = string.Empty;
        UsageLabel.Text = string.Empty;

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
}