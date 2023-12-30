using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

using RazorClassLibrary.helpers;
using System.Net.Http.Json;

namespace RazorClassLibrary.Pages
{
    public partial class UploadFiles : ComponentBase
    {
        [Inject] HttpClient? Http { get; set; }
        [Inject] ILogger<UploadFiles>? Logger { get; set; }
        readonly IList<File> files = new List<File>();
        private IList<UploadResult> uploadResults = new List<UploadResult>();
        readonly int maxAllowedFiles = 1;
        private bool shouldRender;

        protected override bool ShouldRender() => shouldRender;

        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            shouldRender = false;
            long maxFileSize = 1024 * 1024 * 15;
            var upload = false;

            using var content = new MultipartFormDataContent();

            foreach (var file in e.GetMultipleFiles(maxAllowedFiles))
            {
                if (uploadResults.SingleOrDefault(
                    f => f.FileName == file.Name) is null)
                {
                    var fileContent = new StreamContent(file.OpenReadStream());

                    files.Add(
                        new File()
                        {
                            Name = file.Name,
                        });

                    if (file.Size < maxFileSize)
                    {
                        content.Add(
                            content: fileContent,
                            name: "\"files\"",
                            fileName: file.Name);

                        upload = true;
                    }
                    else
                    {
                        Logger!.LogInformation("{FileName} not uploaded", file.Name);

                        uploadResults.Add(
                            new UploadResult()
                            {
                                FileName = file.Name,
                                ErrorCode = 6,
                                Uploaded = false,
                            });
                    }
                }
            }

            if (upload)
            {
                var response = await Http!.PostAsync("/Filesave", content);

                var newUploadResults = await response.Content
                    .ReadFromJsonAsync<IList<UploadResult>>();

                if (newUploadResults != null)
                {
                    uploadResults = uploadResults.Concat(newUploadResults).ToList();
                }
            }

            shouldRender = true;
        }

        private static bool FileUpload(IList<UploadResult> uploadResults,
            string fileName, ILogger<UploadFiles> logger, out UploadResult? result)
        {
            if (fileName == null)
            {
                result = null;
                return false;
            }
            result = uploadResults!.SingleOrDefault(f => f.FileName == fileName);

            if (result is null)
            {
                logger.LogInformation("{FileName} not uploaded", fileName);
                result = new UploadResult() { ErrorCode = 5 };
            }

            return result.Uploaded;
        }

        private class File
        {
            public string? Name { get; set; }
        }
        public string? Name { get; set; }
    }
}