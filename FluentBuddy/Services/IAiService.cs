using FluentBuddy.Models;

namespace FluentBuddy.Services;
public interface IAiService
{
    Task<string> SendChatMessageAsync(string userMessage, string englishLevel, string apiKey);
    Task<GrammarResult> CorrectGrammarAsync(string sentence, string englishLevel, string apiKey);
    Task<WordAnalysisResult> AnalyzeWordAsync(string word, string englishLevel, string apiKey);
    Task<List<QuizQuestion>> GenerateQuizAsync(string topic, string englishLevel, string apiKey);
}