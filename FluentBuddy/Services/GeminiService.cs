using FluentBuddy.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace FluentBuddy.Services;

public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;

    public GeminiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> SendChatMessageAsync(string userMessage, string englishLevel, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return "API key is missing. Please add your Gemini API key in Settings.";

        var prompt =
            $"You are a friendly English tutor for a {englishLevel.ToLower()} learner. " +
            "Reply in English, correct mistakes gently, and explain clearly.\n\n" +
            $"User: {userMessage}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}";

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"Gemini error: {response.StatusCode}\n{responseText}";

            using var jsonDoc = JsonDocument.Parse(responseText);

            var content = jsonDoc
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return content ?? "No response received.";
        }
        catch (Exception ex)
        {
            return $"Request failed: {ex.Message}";
        }
    }

    public async Task<GrammarResult> CorrectGrammarAsync(string sentence, string englishLevel, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return new GrammarResult { Explanation = "Missing API key." };

        var prompt = $@"Correct the following English sentence for a {englishLevel.ToLower()} learner.

Return ONLY raw JSON.
Do NOT use markdown.
Do NOT use code blocks.
Do NOT add extra text.

Use this exact format:
{{
  ""correctedSentence"": ""..."",
  ""explanation"": ""..."",
  ""example"": ""...""
}}

Sentence: {sentence}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}";

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new GrammarResult
                {
                    Explanation = $"Gemini error: {response.StatusCode}\n{responseText}"
                };
            }

            using var jsonDoc = JsonDocument.Parse(responseText);

            var content = jsonDoc
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            content = CleanJson(content);

            var parsed = JsonSerializer.Deserialize<GrammarResult>(content!);

            return parsed ?? new GrammarResult { Explanation = "Parsing failed." };
        }
        catch (Exception ex)
        {
            return new GrammarResult
            {
                Explanation = $"Request failed: {ex.Message}"
            };
        }
    }

    public async Task<WordAnalysisResult> AnalyzeWordAsync(string word, string englishLevel, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return new WordAnalysisResult { Meaning = "Missing API key." };

        var prompt = $@"Analyze the English word ""{word}"" for a {englishLevel.ToLower()} learner.

Return ONLY raw JSON.
Do NOT use markdown.
Do NOT use code blocks.
Do NOT add extra text.

Use this exact format:
{{
  ""word"": ""..."",
  ""meaning"": ""..."",
  ""partOfSpeech"": ""..."",
  ""forms"": [""...""],
  ""wordFamily"": [""...""],
  ""synonyms"": [""...""],
  ""antonyms"": [""...""],
  ""examples"": [""...""],
  ""usage"": ""...""
}}

Rules:
- Keep explanations simple and learner-friendly.
- Include only real and common English forms and related words.
- If a category does not apply, return an empty array.
- Examples should be simple English sentences.";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}";

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new WordAnalysisResult
                {
                    Meaning = $"Gemini error: {response.StatusCode}\n{responseText}"
                };
            }

            using var jsonDoc = JsonDocument.Parse(responseText);

            var content = jsonDoc
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            content = CleanJson(content);

            var parsed = JsonSerializer.Deserialize<WordAnalysisResult>(content!);

            return parsed ?? new WordAnalysisResult
            {
                Meaning = "Parsing failed."
            };
        }
        catch (Exception ex)
        {
            return new WordAnalysisResult
            {
                Meaning = $"Request failed: {ex.Message}"
            };
        }
    }

    public async Task<List<QuizQuestion>> GenerateQuizAsync(string topic, string englishLevel, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return new List<QuizQuestion>();

        var prompt = $@"Create an English quiz for a {englishLevel.ToLower()} learner about ""{topic}"".

Return ONLY raw JSON.
Do NOT use markdown.
Do NOT use code blocks.
Do NOT add extra text.

Return this exact JSON array format:
[
  {{
    ""type"": ""closed"",
    ""question"": ""..."",
    ""options"": [""..."", ""..."", ""..."", ""...""],
    ""correctAnswer"": ""...""
  }},
  {{
    ""type"": ""open"",
    ""question"": ""..."",
    ""options"": [],
    ""correctAnswer"": ""...""
  }}
]

Rules:
- Generate exactly 5 questions.
- Include both closed and open questions.
- Use ""closed"" for multiple choice questions.
- Use ""open"" for free-text questions.
- Closed questions must have exactly 4 options.
- Open questions must have an empty options array.
- The correct answer must be short and clear.
- Keep language appropriate for the learner level.";

        var requestBody = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
        };

        var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}";

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new List<QuizQuestion>();

            using var jsonDoc = JsonDocument.Parse(responseText);

            var content = jsonDoc
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            content = CleanJson(content);

            var parsed = JsonSerializer.Deserialize<List<QuizQuestion>>(content!);

            return parsed ?? new List<QuizQuestion>();
        }
        catch
        {
            return new List<QuizQuestion>();
        }
    }

    private string CleanJson(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var cleaned = input.Trim();

        if (cleaned.StartsWith("```"))
        {
            var firstNewLine = cleaned.IndexOf('\n');
            if (firstNewLine >= 0)
                cleaned = cleaned[(firstNewLine + 1)..];

            if (cleaned.EndsWith("```"))
                cleaned = cleaned[..^3];
        }

        return cleaned.Trim();
    }
}