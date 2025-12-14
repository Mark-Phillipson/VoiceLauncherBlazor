using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RazorClassLibrary.Pages
{
    public partial class FaceRecognition : ComponentBase
    {
        // Maximum file size for uploads (5MB)
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024;
        [Inject] private IFaceImageRepository? FaceImageRepository { get; set; }
        [Inject] private IFaceTagRepository? FaceTagRepository { get; set; }
        [Inject] private IJSRuntime? JSRuntime { get; set; }

        private List<FaceImageDTO> _faceImages = new();
        private FaceImageDTO? _currentImage;
        private int _selectedImageId;
        private bool _isLoadingImages = false;
        private bool _isTagging = false;
        private string _imageName = string.Empty;
        private string _imageDescription = string.Empty;
        private string _newTagName = string.Empty;
        private string _searchName = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private IBrowserFile? _selectedFile;
        private FaceTagDTO? _tempTag;
        private FaceTagDTO? _hoveredTag;
        private ElementReference _imageElement;
        private ElementReference _nameInputElement;

        protected override async Task OnInitializedAsync()
        {
            await LoadAllImages();
        }

        private async Task LoadAllImages()
        {
            _isLoadingImages = true;
            try
            {
                var images = await FaceImageRepository!.GetAllFaceImagesAsync();
                _faceImages = images.ToList();
            }
            catch (Exception ex)
            {
                SetError(ex, "Error loading images");
            }
            finally
            {
                _isLoadingImages = false;
            }
        }

        private void HandleFileSelected(InputFileChangeEventArgs e)
        {
            _selectedFile = e.File;
        }

        private async Task UploadImage()
        {
            if (_selectedFile == null || string.IsNullOrWhiteSpace(_imageName))
            {
                SetError("Please provide an image name and select a file.");
                return;
            }

            try
            {
                using var stream = _selectedFile.OpenReadStream(MaxFileSizeInBytes);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                var base64 = Convert.ToBase64String(imageBytes);

                var newImage = new FaceImageDTO
                {
                    ImageName = _imageName,
                    Description = _imageDescription,
                    ImageData = base64,
                    ContentType = _selectedFile.ContentType,
                    UploadDate = DateTime.Now,
                    FaceTags = new List<FaceTagDTO>()
                };

                var result = await FaceImageRepository!.AddFaceImageAsync(newImage);
                if (result != null)
                {
                    _successMessage = $"Image '{_imageName}' uploaded successfully!";
                    _imageName = string.Empty;
                    _imageDescription = string.Empty;
                    _selectedFile = null;
                    await LoadAllImages();
                    await LoadImage(result.Id);
                }
                else
                {
                    SetError("Failed to upload image.");
                }
            }
            catch (Exception ex)
            {
                SetError(ex, "Error uploading image");
            }
        }

        private async Task LoadImage(int imageId)
        {
            try
            {
                _currentImage = await FaceImageRepository!.GetFaceImageByIdAsync(imageId);
                _selectedImageId = imageId;
                _isTagging = false;
                _tempTag = null;
                _searchName = string.Empty;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                SetError(ex, "Error loading image");
            }
        }

        private async Task DeleteCurrentImage()
        {
            if (_currentImage == null) return;

            var confirmed = await JSRuntime!.InvokeAsync<bool>("confirm", $"Are you sure you want to delete '{_currentImage.ImageName}'?");
            if (!confirmed) return;

            try
            {
                var success = await FaceImageRepository!.DeleteFaceImageAsync(_currentImage.Id);
                if (success)
                {
                    _successMessage = $"Image '{_currentImage.ImageName}' deleted successfully!";
                    _currentImage = null;
                    _selectedImageId = 0;
                    await LoadAllImages();
                }
                else
                {
                    SetError("Failed to delete image.");
                }
            }
            catch (Exception ex)
            {
                SetError(ex, "Error deleting image");
            }
        }

        private void StartTagging()
        {
            _isTagging = true;
            _newTagName = string.Empty;
        }

        private void CancelTagging()
        {
            _isTagging = false;
            _tempTag = null;
            _newTagName = string.Empty;
        }

        private async Task OnImageClick(MouseEventArgs e)
        {
            if (!_isTagging || _currentImage == null) return;

            try
            {
                // Try to get image dimensions and click position using the faceRecognition helper.
                BoundingRect rect = null;
                try
                {
                    rect = await JSRuntime!.InvokeAsync<BoundingRect>("faceRecognitionHelpers.getImageBoundingRect", _currentImage.ImageName);
                }
                catch (JSException)
                {
                    // Helper not available; fall back to using element reference with a minimal helper
                    rect = await JSRuntime!.InvokeAsync<BoundingRect>("blazorHelpers.getBoundingClientRect", _imageElement);
                }

                if (rect == null)
                {
                    Console.WriteLine("Could not get image bounding rect");
                    SetError("Could not locate image element.");
                    return;
                }

                // Calculate click position as percentage
                var xPercent = ((e.ClientX - rect.Left) / rect.Width) * 100;
                var yPercent = ((e.ClientY - rect.Top) / rect.Height) * 100;

                // Create a default face box (10% width/height) centered on click
                var boxWidth = 10.0;
                var boxHeight = 10.0;
                var xPos = Math.Max(0, Math.Min(xPercent - boxWidth / 2, 100 - boxWidth));
                var yPos = Math.Max(0, Math.Min(yPercent - boxHeight / 2, 100 - boxHeight));

                _tempTag = new FaceTagDTO
                {
                    X = xPos,
                    Y = yPos,
                    Width = boxWidth,
                    Height = boxHeight,
                    FaceImageId = _currentImage.Id
                };

                StateHasChanged();

                // Focus the name input using safe JS helper (fall back to focusing the input element by ref)
                await Task.Delay(100);
                try
                {
                    await JSRuntime!.InvokeVoidAsync("faceRecognitionHelpers.focusNameInput");
                }
                catch (JSException)
                {
                    await JSRuntime!.InvokeVoidAsync("blazorHelpers.focusElement", _nameInputElement);
                }
            }
            catch (Exception ex)
            {
                SetError(ex, "Error creating tag");
            }
        }

        private async Task SaveTag()
        {
            if (_tempTag == null || string.IsNullOrWhiteSpace(_newTagName) || _currentImage == null) return;

            try
            {
                _tempTag.FirstName = _newTagName.Trim();
                var result = await FaceTagRepository!.AddFaceTagAsync(_tempTag);
                
                if (result != null)
                {
                    _successMessage = $"Tagged '{_newTagName}' successfully!";
                    _isTagging = false;
                    _tempTag = null;
                    _newTagName = string.Empty;
                    
                    // Reload the current image to show the new tag
                    await LoadImage(_currentImage.Id);
                }
                else
                {
                    SetError("Failed to save tag.");
                }
            }
            catch (Exception ex)
            {
                SetError(ex, "Error saving tag");
            }
        }

        private async Task DeleteTag(int tagId)
        {
            if (_currentImage == null) return;

            var confirmed = await JSRuntime!.InvokeAsync<bool>("confirm", "Are you sure you want to delete this tag?");
            if (!confirmed) return;

            try
            {
                var success = await FaceTagRepository!.DeleteFaceTagAsync(tagId);
                if (success)
                {
                    _successMessage = "Tag deleted successfully!";
                    await LoadImage(_currentImage.Id);
                }
                else
                {
                    SetError("Failed to delete tag.");
                }
            }
            catch (Exception ex)
            {
                SetError(ex, "Error deleting tag");
            }
        }

        private void SetError(Exception ex, string context)
        {
            Console.WriteLine($"{context}: {ex}");
            // Include the exception message and full ToString for details
            _errorMessage = $"{context}: {ex.Message}\n{ex}";
            StateHasChanged();
        }

        private void SetError(string message)
        {
            Console.WriteLine($"Error: {message}");
            _errorMessage = message;
            StateHasChanged();
        }

        private void ShowTagName(FaceTagDTO tag)
        {
            _hoveredTag = tag;
            StateHasChanged();
        }

        private void HideTagName()
        {
            _hoveredTag = null;
            StateHasChanged();
        }

        private string GetImageSource()
        {
            if (_currentImage == null || string.IsNullOrEmpty(_currentImage.ImageData))
                return string.Empty;

            var contentType = string.IsNullOrEmpty(_currentImage.ContentType) ? "image/jpeg" : _currentImage.ContentType;
            return $"data:{contentType};base64,{_currentImage.ImageData}";
        }

        private class BoundingRect
        {
            public double Left { get; set; }
            public double Top { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
        }
    }
}
