namespace FluentBuddy.Models;

public class AppSettings
{
    public string Provider { get; set; } = "OpenAI";
    public string EnglishLevel { get; set; } = "Beginner";
    public string ApiKey { get; set; } = string.Empty;
}