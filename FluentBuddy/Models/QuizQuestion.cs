using System.Text.Json.Serialization;

namespace FluentBuddy.Models;

public class QuizQuestion
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "closed" or "open"

    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = new();

    [JsonPropertyName("correctAnswer")]
    public string CorrectAnswer { get; set; } = string.Empty;
}