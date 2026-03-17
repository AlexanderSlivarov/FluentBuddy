using System.Text.Json.Serialization;

namespace FluentBuddy.Models;

public class WordAnalysisResult
{
    [JsonPropertyName("word")]
    public string Word { get; set; } = string.Empty;

    [JsonPropertyName("meaning")]
    public string Meaning { get; set; } = string.Empty;

    [JsonPropertyName("partOfSpeech")]
    public string PartOfSpeech { get; set; } = string.Empty;

    [JsonPropertyName("forms")]
    public List<string> Forms { get; set; } = new();

    [JsonPropertyName("wordFamily")]
    public List<string> WordFamily { get; set; } = new();

    [JsonPropertyName("synonyms")]
    public List<string> Synonyms { get; set; } = new();

    [JsonPropertyName("antonyms")]
    public List<string> Antonyms { get; set; } = new();

    [JsonPropertyName("examples")]
    public List<string> Examples { get; set; } = new();

    [JsonPropertyName("usage")]
    public string Usage { get; set; } = string.Empty;
}