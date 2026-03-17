using FluentBuddy.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FluentBuddy.Services;

public class OpenAiService : IAiService
{
    private readonly HttpClient _httpClient;

    public OpenAiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> SendChatMessageAsync(string userMessage, string englishLevel, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return "API key is missing. Please add your OpenAI API key in Settings.";

        var systemPrompt =
            $"You are a friendly English tutor for a {englishLevel.ToLower()} learner. " +
            "Reply in English, keep the explanation clear, correct mistakes gently, and encourage the learner.";

        var requestBody = new
        {
            model = "gpt-4.1-mini",
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            temperature = 0.7
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(requestBody);

        try
        {
            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"OpenAI error: {response.StatusCode}\n{responseText}";

            using var jsonDoc = JsonDocument.Parse(responseText);

            var content = jsonDoc
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
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
            model = "gpt-4.1-mini",
            messages = new object[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(requestBody);

        try
        {
            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new GrammarResult
                {
                    Explanation = $"OpenAI error: {response.StatusCode}\n{responseText}"
                };
            }

            using var jsonDoc = JsonDocument.Parse(responseText);

            var content = jsonDoc
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
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
            model = "gpt-4.1-mini",
            messages = new object[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.3
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(requestBody);

        try
        {
            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new WordAnalysisResult
                {
                    Meaning = $"OpenAI error: {response.StatusCode}\n{responseText}"
                };
            }

            using var jsonDoc = JsonDocument.Parse(responseText);

            var content = jsonDoc
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
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
            model = "gpt-4.1-mini",
            messages = new object[]
            {
            new { role = "user", content = prompt }
            },
            temperature = 0.4
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(requestBody);

        try
        {
            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return new List<QuizQuestion>();

            using var jsonDoc = JsonDocument.Parse(responseText);

            var content = jsonDoc
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
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