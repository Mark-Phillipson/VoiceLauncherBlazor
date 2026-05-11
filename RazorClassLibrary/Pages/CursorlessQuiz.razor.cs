using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using RazorClassLibrary.Models;
using RazorClassLibrary.Services;

namespace RazorClassLibrary.Pages;

public partial class CursorlessQuiz : ComponentBase
{
    private const string ScoreStorageKey = "cursorlessQuizScores";

    [Inject]
    public IQuizService QuizService { get; set; } = default!;

    [Inject]
    public IJSRuntime JavaScriptRuntime { get; set; } = default!;

    private IReadOnlyList<QuizQuestion> _questions = Array.Empty<QuizQuestion>();
    private List<QuizScore> _scoreHistory = new();
    private int _currentQuestionIndex;
    private int _correctAnswers;
    private int QuestionCount { get; set; } = 10;
    private bool _quizActive;
    private bool _showingResults;
    private bool _isLoadingHistory = true;
    private string? _feedbackMessage;
    private bool _lastAnswerCorrect;
    private bool _waitingForFeedbackDismissal;
    private bool _processingAnswer;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadScoresAsync();
            _isLoadingHistory = false;
            StateHasChanged();
        }
    }

    private QuizQuestion? CurrentQuestion => _quizActive && _currentQuestionIndex < _questions.Count
        ? _questions[_currentQuestionIndex]
        : null;

    private async Task StartQuizAsync()
    {
        _questions = await QuizService.GenerateQuestionsAsync(QuestionCount);
        _currentQuestionIndex = 0;
        _correctAnswers = 0;
        _feedbackMessage = null;
        _lastAnswerCorrect = false;
        _waitingForFeedbackDismissal = false;
        _quizActive = _questions.Count > 0;
        _showingResults = _questions.Count == 0;

        if (_showingResults)
        {
            await SaveCurrentScoreAsync();
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task SubmitAnswerAsync(int choiceIndex)
    {
        var question = CurrentQuestion;
        if (question is null)
        {
            return;
        }

        Console.WriteLine($"SubmitAnswerAsync called: choiceIndex={choiceIndex}, currentIndex={_currentQuestionIndex}, questions={_questions?.Count}, choices={(question.Choices?.Count ?? 0)}");

        // Ignore submissions while another answer is being processed
        if (_processingAnswer)
        {
            Console.WriteLine($"SubmitAnswerAsync: ignored re-entrant submission. index={choiceIndex}, currentIndex={_currentQuestionIndex}");
            return;
        }

        // Validate index bounds to avoid ArgumentOutOfRangeException
        var choicesCount = question.Choices?.Count ?? 0;
        if (choiceIndex < 0 || choiceIndex >= choicesCount)
        {
            Console.WriteLine($"SubmitAnswerAsync: invalid index. index={choiceIndex}, choices={(question.Choices?.Count ?? 0)}, currentIndex={_currentQuestionIndex}");
            return;
        }

        _processingAnswer = true;
        try
        {
            var choices = question.Choices ?? new List<string>();
            var selectedChoice = choices[choiceIndex];
            Console.WriteLine($"SubmitAnswerAsync: selected='{selectedChoice}', correct='{question.CorrectAnswer}'");
            _lastAnswerCorrect = string.Equals(selectedChoice, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            if (_lastAnswerCorrect)
            {
                _correctAnswers++;
            }

            _feedbackMessage = _lastAnswerCorrect
                ? $"Correct. {question.CorrectAnswer}"
                : $"Not quite. Correct answer: {question.CorrectAnswer}";
            _waitingForFeedbackDismissal = !_lastAnswerCorrect;

            await InvokeAsync(StateHasChanged);

            if (_lastAnswerCorrect)
            {
                await Task.Delay(700);
                await MoveToNextQuestionAsync();
            }
        }
        finally
        {
            _processingAnswer = false;
        }
    }

    private async Task MoveToNextQuestionAsync()
    {
        _currentQuestionIndex++;

        Console.WriteLine($"MoveToNextQuestionAsync: newIndex={_currentQuestionIndex}, total={_questions?.Count}");

        if (_currentQuestionIndex >= _questions.Count)
        {
            await FinishQuizAsync();
            return;
        }

        _feedbackMessage = null;
        await InvokeAsync(StateHasChanged);
    }

    private async Task FinishQuizAsync()
    {
        _quizActive = false;
        _showingResults = true;
        _feedbackMessage = null;
        _waitingForFeedbackDismissal = false;

        await SaveCurrentScoreAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task QuitQuizAsync()
    {
        if (!_quizActive)
        {
            return;
        }

        await FinishQuizAsync();
    }

    private async Task ResetToStartAsync()
    {
        _quizActive = false;
        _showingResults = false;
        _questions = Array.Empty<QuizQuestion>();
        _currentQuestionIndex = 0;
        _correctAnswers = 0;
        _feedbackMessage = null;
        _lastAnswerCorrect = false;
        _waitingForFeedbackDismissal = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadScoresAsync()
    {
        try
        {
            var json = await JavaScriptRuntime.InvokeAsync<string?>("localStorage.getItem", ScoreStorageKey);
            _scoreHistory = QuizService.DeserializeScores(json)
                .OrderByDescending(score => score.PlayedAt)
                .ToList();
        }
        catch
        {
            _scoreHistory = new List<QuizScore>();
        }
    }

    private async Task SaveCurrentScoreAsync()
    {
        var playedAt = DateTime.UtcNow;
        var currentScore = new QuizScore
        {
            PlayedAt = playedAt,
            TotalQuestions = _questions.Count,
            CorrectAnswers = _correctAnswers
        };

        _scoreHistory.Insert(0, currentScore);
        _scoreHistory = _scoreHistory
            .OrderByDescending(score => score.PlayedAt)
            .Take(20)
            .ToList();

        await JavaScriptRuntime.InvokeVoidAsync("localStorage.setItem", ScoreStorageKey, QuizService.SerializeScores(_scoreHistory));
    }

    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        var rawKey = args.Key ?? string.Empty;

        if (!_quizActive || CurrentQuestion is null)
        {
            if (_showingResults && IsEnterOrSpace(rawKey))
            {
                await StartQuizAsync();
            }

            return;
        }

        // Ignore key input while an answer is actively being processed
        if (_processingAnswer)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(_feedbackMessage))
        {
            if (IsEnterOrSpace(rawKey) || rawKey.Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                await ContinueAfterFeedbackAsync();
            }

            return;
        }

        var key = rawKey.ToUpperInvariant();
        var answerIndex = key switch
        {
            "DIGIT1" => 0,
            "1" or "NUMPAD1" => 0,
            "DIGIT2" => 1,
            "2" or "NUMPAD2" => 1,
            "DIGIT3" => 2,
            "3" or "NUMPAD3" => 2,
            "DIGIT4" => 3,
            "4" or "NUMPAD4" => 3,
            "Q" or "ESCAPE" => -2,
            _ => -1
        };

        if (answerIndex == -2)
        {
            await QuitQuizAsync();
            return;
        }

        if (answerIndex >= 0 && answerIndex < CurrentQuestion.Choices.Count)
        {
            await SubmitAnswerAsync(answerIndex);
        }
    }

    private static string GetChoiceLabel(int index)
    {
        return $"{index + 1}.";
    }

    private static string GetChoiceAccessKey(int index)
    {
        return index switch
        {
            0 => "1",
            1 => "2",
            2 => "3",
            3 => "4",
            _ => string.Empty
        };
    }

    private async Task ContinueAfterFeedbackAsync()
    {
        if (string.IsNullOrWhiteSpace(_feedbackMessage))
        {
            return;
        }

        await MoveToNextQuestionAsync();
    }

    private static bool IsEnterOrSpace(string key)
    {
        return key.Equals("Enter", StringComparison.OrdinalIgnoreCase)
            || key.Equals(" ", StringComparison.OrdinalIgnoreCase)
            || key.Equals("Spacebar", StringComparison.OrdinalIgnoreCase);
    }
}