using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VoiceAdmin.Components.Pages
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
                Console.WriteLine($"Error loading images: {ex}");
                _errorMessage = "Error loading images. Please try again.";
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
                _errorMessage = "Please provide an image name and select a file.";
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
                    _errorMessage = "Failed to upload image.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image: {ex}");
                _errorMessage = "Error uploading image. Please check the file size and format.";
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
                Console.WriteLine($"Error loading image: {ex}");
                _errorMessage = "Error loading image. Please try again.";
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
                    _errorMessage = "Failed to delete image.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting image: {ex}");
                _errorMessage = "Error deleting image. Please try again.";
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
                // Get image dimensions and click position using safe JS helper
                var rect = await JSRuntime!.InvokeAsync<BoundingRect>(
                    "faceRecognitionHelpers.getImageBoundingRect", 
                    _currentImage.ImageName);

                if (rect == null)
                {
                    Console.WriteLine("Could not get image bounding rect");
                    _errorMessage = "Error: Could not locate image element.";
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

                // Focus the name input using safe JS helper
                await Task.Delay(100);
                await JSRuntime.InvokeVoidAsync("faceRecognitionHelpers.focusNameInput");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating tag: {ex}");
                _errorMessage = "Error creating tag. Please try again.";
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
                    _errorMessage = "Failed to save tag.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving tag: {ex}");
                _errorMessage = "Error saving tag. Please try again.";
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
                    _errorMessage = "Failed to delete tag.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting tag: {ex}");
                _errorMessage = "Error deleting tag. Please try again.";
            }
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
