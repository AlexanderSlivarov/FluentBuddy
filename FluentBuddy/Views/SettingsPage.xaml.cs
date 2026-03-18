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

    private async void OnSaveSettingsClicked(object sender, EventArgs e)
    {
        try
        {
            SaveButton.IsEnabled = false;
            SaveButton.Text = "Saving...";

            var settings = new AppSettings
            {
                Provider = ProviderPicker.SelectedItem?.ToString() ?? "OpenAI",
                EnglishLevel = LevelPicker.SelectedItem?.ToString() ?? "Beginner",
                ApiKey = ApiKeyEntry.Text ?? string.Empty
            };

            _settingsService.SaveSettings(settings);

            StatusLabel.Text = "Settings saved successfully.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
        }
        finally
        {
            SaveButton.IsEnabled = true;
            SaveButton.Text = "Save Settings";
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync("//home");
        }
            
        else
        {
            await Navigation.PopAsync();
        }            
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