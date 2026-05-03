using System.Threading;
using System.Threading.Tasks;

namespace VoiceAdmin.Services
{
    public class ImageGenerationResult
    {
        public string Filename { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        // The prompt text that was sent to the provider (useful for debugging)
        public string Prompt { get; set; } = string.Empty;
        // Raw provider response body or additional debugging info
        public string ProviderResponse { get; set; } = string.Empty;
        // Optional: provider/model used for image generation
        public string Model { get; set; } = string.Empty;
    }

    public interface IImageGenerationService
    {
        Task<ImageGenerationResult> GenerateImageAsync(string prompt, int width = 1024, int height = 1024, CancellationToken cancellationToken = default);
    }
}
