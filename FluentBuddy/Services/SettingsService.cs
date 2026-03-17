using FluentBuddy.Models;

namespace FluentBuddy.Services;

public class SettingsService
{
    private const string ProviderKey = "provider";
    private const string EnglishLevelKey = "english_level";
    private const string ApiKeyKey = "api_key";

    public AppSettings GetSettings()
    {
        return new AppSettings
        {
            Provider = Preferences.Get(ProviderKey, "OpenAI"),
            EnglishLevel = Preferences.Get(EnglishLevelKey, "Beginner"),
            ApiKey = Preferences.Get(ApiKeyKey, string.Empty)
        };
    }

    public void SaveSettings(AppSettings settings)
    {
        Preferences.Set(ProviderKey, settings.Provider);
        Preferences.Set(EnglishLevelKey, settings.EnglishLevel);
        Preferences.Set(ApiKeyKey, settings.ApiKey);
    }
}