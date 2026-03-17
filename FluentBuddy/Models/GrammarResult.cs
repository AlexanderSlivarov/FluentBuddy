using System.Text.Json.Serialization;

namespace FluentBuddy.Models;

public class GrammarResult
{
    [JsonPropertyName("correctedSentence")]
    public string CorrectedSentence { get; set; } = string.Empty;

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [JsonPropertyName("example")]
    public string ExtraExample { get; set; } = string.Empty;
}