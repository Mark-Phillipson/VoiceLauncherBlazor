using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace VoiceLauncherBlazor.Controllers
{
    [Route("api/uploads")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        [HttpGet("{filename}")]
        public IActionResult GetImage(string filename)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            var filePath = Path.Combine(uploadsPath, filename);
            if (!System.IO.File.Exists(filePath))
                return NotFound();
            var ext = Path.GetExtension(filename).ToLowerInvariant();
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, contentType);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploadsPath);
            var guid = Guid.NewGuid().ToString();
            var fileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{guid}_{fileName}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return Ok(new { url = $"/uploads/{uniqueFileName}" });
        }
    }
}
