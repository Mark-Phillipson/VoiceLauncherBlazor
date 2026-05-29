using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using RazorClassLibrary.Models;

namespace RazorClassLibrary.Services;

public interface IAIReviewService
{
    /// <summary>
    /// Enqueue a set of questions for asynchronous AI review.
    /// </summary>
    Task EnqueueForReviewAsync(IEnumerable<QuizQuestion> questions, CancellationToken ct = default);

    /// <summary>
    /// Synchronously ask the AI to review and (optionally) rewrite the provided questions.
    /// Returns a list of review results corresponding to the input questions.
    /// </summary>
    Task<IReadOnlyList<Models.AIReviewResult>> ReviewQuestionsAsync(IEnumerable<QuizQuestion> questions, CancellationToken ct = default);

    /// <summary>
    /// Exposes an internal reader for background workers to consume queued batches.
    /// </summary>
    ChannelReader<QuizQuestion[]> GetReviewReader();
}
