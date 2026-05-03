using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VoiceAdmin.Services
{
    // Minimal placeholder implementation that creates a simple PNG image locally.
    // This avoids adding an external provider dependency while providing end-to-end behavior.
    public class PlaceholderImageGenerationService : IImageGenerationService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<PlaceholderImageGenerationService> _logger;
        private readonly IConfiguration _config;

        public PlaceholderImageGenerationService(IWebHostEnvironment env, ILogger<PlaceholderImageGenerationService> logger, IConfiguration config)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config;
        }

        public async Task<ImageGenerationResult> GenerateImageAsync(string prompt, int width = 1024, int height = 1024, CancellationToken cancellationToken = default)
        {
            // Create images folder under web root
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var imagesDir = Path.Combine(webRoot, "images");
            Directory.CreateDirectory(imagesDir);

            var id = Guid.NewGuid().ToString("N");
            var filename = $"launcher-{id}.png";
            var thumbFilename = $"launcher-{id}-thumb.png";
            var filePath = Path.Combine(imagesDir, filename);
            var thumbPath = Path.Combine(imagesDir, thumbFilename);

            try
            {
                using (var image = new Image<Rgba32>(width, height))
                {
                    // Simple background - use a deterministic color derived from prompt hash
                    var hash = prompt?.GetHashCode() ?? Environment.TickCount;
                    var r = (byte)((hash >> 16) & 0xFF);
                    var g = (byte)((hash >> 8) & 0xFF);
                    var b = (byte)(hash & 0xFF);
                    image.Mutate(x => x.BackgroundColor(new Rgba32(r, g, b)));

                    // Save main image
                    await image.SaveAsPngAsync(filePath, cancellationToken);

                    // Create thumbnail (256x256)
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(256, 256),
                        Mode = ResizeMode.Crop
                    }));
                    await image.SaveAsPngAsync(thumbPath, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate placeholder image");
                throw;
            }

            return new ImageGenerationResult
            {
                Filename = filename,
                Thumbnail = thumbFilename,
                Prompt = prompt ?? string.Empty,
                ProviderResponse = "placeholder-generated",
                Model = "placeholder"
            };
        }
    }
}
