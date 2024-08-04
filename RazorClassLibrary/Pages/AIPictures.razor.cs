using Microsoft.AspNetCore.Components;
using OpenAI.Images;
using VoiceLauncher;
namespace RazorClassLibrary.Pages
{
    public partial class AIPictures : ComponentBase
    {
        string imagePrompt = "";
        string imageUrl = "";
        bool generatingImage = false;
        private async Task GenerateImage()
        {
            if (!string.IsNullOrWhiteSpace(imagePrompt))
            {
                imageUrl = "";
                generatingImage = true;
                var openAI = new ImageClient("dall-e-3", Constants.OpenAIAPIKEY);
                var imageRequest = new ImageGenerationOptions
                {
                    Quality = GeneratedImageQuality.High,
                    Size = GeneratedImageSize.W1024xH1024,
                    Style = GeneratedImageStyle.Vivid,
                    ResponseFormat = GeneratedImageFormat.Uri
                };

                var response = await openAI.GenerateImageAsync(imagePrompt, imageRequest);
                System.Console.WriteLine(response.Value.ImageUri);
                imageUrl = response.Value.ImageUri.ToString();
                generatingImage = false;
                StateHasChanged();
            }
        }
    }
}