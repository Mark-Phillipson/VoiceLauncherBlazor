using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace VoiceAdmin.Services
{
    public class OpenAIImageGenerationService : IImageGenerationService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<OpenAIImageGenerationService> _logger;
        private readonly IConfiguration _config;

        public OpenAIImageGenerationService(IWebHostEnvironment env, ILogger<OpenAIImageGenerationService> logger, IConfiguration config)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<ImageGenerationResult> GenerateImageAsync(string prompt, int width = 1024, int height = 1024, CancellationToken cancellationToken = default)
        {
            var apiKey = _config["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("OpenAI API key not configured (OpenAI:ApiKey or OPENAI_API_KEY).");
            }

            // Use the public OpenAI images endpoint by default
            var requestUri = new Uri("https://api.openai.com/v1/images/generations");

            var imageModel = _config["OpenAI:ImageModel"] ?? Environment.GetEnvironmentVariable("OPENAI_IMAGE_MODEL");

            var payload = new System.Collections.Generic.Dictionary<string, object>
            {
                ["prompt"] = prompt ?? string.Empty,
                ["n"] = 1,
                ["size"] = $"{width}x{height}",
                ["response_format"] = "b64_json"
            };

            if (!string.IsNullOrWhiteSpace(imageModel))
            {
                payload["model"] = imageModel;
                _logger.LogInformation("OpenAI image generation: using model {Model}", imageModel);
            }

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI image generation failed ({Status}): {Body}", response.StatusCode, responseBody);
                throw new Exception($"Image generation failed: {response.StatusCode}: {responseBody}");
            }

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                if (root.TryGetProperty("data", out var dataArr) && dataArr.GetArrayLength() > 0)
                {
                    var first = dataArr[0];
                    if (first.TryGetProperty("b64_json", out var b64Elem))
                    {
                        var b64 = b64Elem.GetString();
                        if (b64 == null) throw new Exception("OpenAI response missing b64_json content.");
                        var bytes = Convert.FromBase64String(b64);

                        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                        var imagesDir = Path.Combine(webRoot, "images");
                        Directory.CreateDirectory(imagesDir);

                        var id = Guid.NewGuid().ToString("N");
                        var filename = $"launcher-{id}.png";
                        var thumbFilename = $"launcher-{id}-thumb.png";
                        var filePath = Path.Combine(imagesDir, filename);
                        var thumbPath = Path.Combine(imagesDir, thumbFilename);

                        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);

                        using (var image = Image.Load(bytes))
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions { Size = new SixLabors.ImageSharp.Size(256, 256), Mode = ResizeMode.Crop }));
                            await image.SaveAsPngAsync(thumbPath, cancellationToken);
                        }

                        return new ImageGenerationResult { Filename = filename, Thumbnail = thumbFilename, Prompt = prompt ?? string.Empty, ProviderResponse = responseBody, Model = imageModel ?? string.Empty };
                    }
                    else if (first.TryGetProperty("url", out var urlElem))
                    {
                        var url = urlElem.GetString();
                        if (string.IsNullOrWhiteSpace(url)) throw new Exception("OpenAI returned empty URL for image");

                        // Download image bytes
                        var imageBytes = await client.GetByteArrayAsync(url, cancellationToken);

                        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                        var imagesDir = Path.Combine(webRoot, "images");
                        Directory.CreateDirectory(imagesDir);

                        var id = Guid.NewGuid().ToString("N");
                        var filename = $"launcher-{id}.png";
                        var thumbFilename = $"launcher-{id}-thumb.png";
                        var filePath = Path.Combine(imagesDir, filename);
                        var thumbPath = Path.Combine(imagesDir, thumbFilename);

                        await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

                        using (var image = Image.Load(imageBytes))
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions { Size = new SixLabors.ImageSharp.Size(256, 256), Mode = ResizeMode.Crop }));
                            await image.SaveAsPngAsync(thumbPath, cancellationToken);
                        }

                        return new ImageGenerationResult { Filename = filename, Thumbnail = thumbFilename, Prompt = prompt ?? string.Empty, ProviderResponse = responseBody, Model = imageModel ?? string.Empty };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse OpenAI image response: {Body}", responseBody);
                throw new Exception($"Failed to parse OpenAI response: {ex.Message}. Body: {responseBody}", ex);
            }

            throw new Exception("OpenAI image response did not contain expected data");
        }
    }
}
