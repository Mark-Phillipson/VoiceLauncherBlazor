using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using RazorClassLibrary.Services;

namespace VoiceAdmin.Services;

public class AIReviewBackgroundService : BackgroundService
{
    private readonly IAIReviewService _reviewService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AIReviewBackgroundService> _logger;

    public AIReviewBackgroundService(IAIReviewService reviewService, IWebHostEnvironment env, ILogger<AIReviewBackgroundService> logger)
    {
        _reviewService = reviewService;
        _env = env;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = _reviewService.GetReviewReader();
        _logger.LogInformation("AIReviewBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var canRead = await reader.WaitToReadAsync(stoppingToken).ConfigureAwait(false);
                if (!canRead) break;

                while (reader.TryRead(out var batch))
                {
                    try
                    {
                        var results = await _reviewService.ReviewQuestionsAsync(batch, stoppingToken).ConfigureAwait(false);

                        var outDir = Path.Combine(_env.ContentRootPath, "App_Data", "ai-review-results");
                        Directory.CreateDirectory(outDir);
                        var file = Path.Combine(outDir, $"ai-review-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.json");
                        await File.WriteAllTextAsync(file, JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }), stoppingToken).ConfigureAwait(false);
                        _logger.LogInformation("Wrote AI review results to {file}", file);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception exBatch)
                    {
                        _logger.LogError(exBatch, "Error processing AI review batch");
                    }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AIReviewBackgroundService encountered an error; sleeping briefly before retrying");
                try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false); } catch { }
            }
        }

        _logger.LogInformation("AIReviewBackgroundService stopping");
    }
}
