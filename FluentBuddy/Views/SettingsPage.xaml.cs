using FluentBuddy.Models;
using FluentBuddy.Services;

namespace FluentBuddy.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsService _settingsService;

    public SettingsPage()
    {
        InitializeComponent();
        _settingsService = new SettingsService();
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _settingsService.GetSettings();

        ProviderPicker.SelectedItem = settings.Provider;
        LevelPicker.SelectedItem = settings.EnglishLevel;
        ApiKeyEntry.Text = settings.ApiKey;
    }

    private void OnSaveSettingsClicked(object sender, EventArgs e)
    {
        var settings = new AppSettings
        {
            Provider = ProviderPicker.SelectedItem?.ToString() ?? "OpenAI",
            EnglishLevel = LevelPicker.SelectedItem?.ToString() ?? "Beginner",
            ApiKey = ApiKeyEntry.Text ?? string.Empty
        };

        _settingsService.SaveSettings(settings);
        StatusLabel.Text = "Settings saved successfully.";
    }
}