using FluentBuddy.Models;
using FluentBuddy.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FluentBuddy.Views;

public partial class QuizPage : ContentPage
{
    private readonly SettingsService _settingsService;
    private readonly ObservableCollection<QuizQuestionDisplay> _questions = new();

    public QuizPage()
    {
        InitializeComponent();
        _settingsService = new SettingsService();
        QuizCollectionView.ItemsSource = _questions;
    }

    private async void OnGenerateQuizClicked(object sender, EventArgs e)
    {
        var topic = TopicEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(topic))
        {
            StatusLabel.Text = "Please enter a topic.";
            ScoreLabel.Text = string.Empty;
            _questions.Clear();
            return;
        }

        StatusLabel.Text = "Loading...";
        ScoreLabel.Text = string.Empty;
        _questions.Clear();

        var settings = _settingsService.GetSettings();

        IAiService aiService = settings.Provider == "Gemini"
            ? new GeminiService()
            : new OpenAiService();

        var quiz = await aiService.GenerateQuizAsync(topic, settings.EnglishLevel, settings.ApiKey);

        if (quiz.Count == 0)
        {
            StatusLabel.Text = "No quiz questions were generated.";
            return;
        }

        foreach (var item in quiz.Select((q, index) => new QuizQuestionDisplay
        {
            Question = $"{index + 1}. {q.Question}",
            Type = (q.Type ?? "closed").ToLower(),
            Options = q.Options ?? new List<string>(),
            CorrectAnswer = q.CorrectAnswer ?? string.Empty,
            ResultColor = Colors.Transparent
        }))
        {
            _questions.Add(item);
        }

        StatusLabel.Text = $"Generated {_questions.Count} questions.";
    }

    private void OnCheckQuizClicked(object sender, EventArgs e)
    {
        if (_questions.Count == 0)
        {
            StatusLabel.Text = "Generate a quiz first.";
            ScoreLabel.Text = string.Empty;
            return;
        }

        int correctCount = 0;

        foreach (var question in _questions)
        {
            var userAnswer = question.Type == "open"
                ? question.UserOpenAnswer
                : question.SelectedOption;

            var isCorrect = AreAnswersEquivalent(userAnswer, question.CorrectAnswer);

            question.ShowResult = true;

            if (isCorrect)
            {
                question.ResultText = $"? Correct. Answer: {Capitalize(question.CorrectAnswer)}";
                question.ResultColor = Colors.LightGreen;
                correctCount++;
            }
            else
            {
                var shownUserAnswer = string.IsNullOrWhiteSpace(userAnswer)
                    ? "No answer"
                    : Capitalize(userAnswer);

                question.ResultText =
                    $"? Incorrect.\nYour answer: {shownUserAnswer}\nCorrect answer: {Capitalize(question.CorrectAnswer)}";

                question.ResultColor = Colors.IndianRed;
            }
        }

        var percentage = (int)Math.Round((double)correctCount / _questions.Count * 100);
        ScoreLabel.Text = $"Score: {correctCount}/{_questions.Count} ({percentage}%)";
        StatusLabel.Text = "Quiz evaluated.";
    }

    private static bool AreAnswersEquivalent(string? userAnswer, string? correctAnswer)
    {
        var left = NormalizeAnswer(userAnswer);
        var right = NormalizeAnswer(correctAnswer);

        return !string.IsNullOrWhiteSpace(left) &&
               !string.IsNullOrWhiteSpace(right) &&
               left == right;
    }

    private static string NormalizeAnswer(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var cleaned = input.Trim().ToLowerInvariant();

        cleaned = cleaned
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(";", "")
            .Replace(":", "")
            .Replace("\"", "")
            .Replace("'", "");

        while (cleaned.Contains("  "))
            cleaned = cleaned.Replace("  ", " ");

        return cleaned;
    }

    private static string Capitalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        text = text.Trim();

        return text.Length == 1
            ? text.ToUpper()
            : char.ToUpper(text[0]) + text[1..];
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//settings");
    }

    private class QuizQuestionDisplay : INotifyPropertyChanged
    {
        private string _selectedOption = string.Empty;
        private string _userOpenAnswer = string.Empty;
        private string _resultText = string.Empty;
        private bool _showResult;
        private Color _resultColor = Colors.Transparent;

        public string Question { get; set; } = string.Empty;
        public string Type { get; set; } = "closed";
        public List<string> Options { get; set; } = new();
        public string CorrectAnswer { get; set; } = string.Empty;

        public string SelectedOption
        {
            get => _selectedOption;
            set
            {
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    OnPropertyChanged();
                }
            }
        }

        public string UserOpenAnswer
        {
            get => _userOpenAnswer;
            set
            {
                if (_userOpenAnswer != value)
                {
                    _userOpenAnswer = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ResultText
        {
            get => _resultText;
            set
            {
                if (_resultText != value)
                {
                    _resultText = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowResult
        {
            get => _showResult;
            set
            {
                if (_showResult != value)
                {
                    _showResult = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color ResultColor
        {
            get => _resultColor;
            set
            {
                if (_resultColor != value)
                {
                    _resultColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsClosed => Type == "closed";
        public bool IsOpen => Type == "open";

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}