using Microsoft.AspNetCore.Mvc;
using VoiceAdmin.Services;

namespace VoiceAdmin.Controllers
{
    [ApiController]
    [Route("api/clipboard")]
    public class ClipboardController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null) return BadRequest("No file");
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var data = ms.ToArray();
            var token = Guid.NewGuid().ToString("N");
            TempImageStore.Set(token, data, file.ContentType ?? "application/octet-stream");
            return Ok(new { token });
        }

        [HttpGet("getbase64/{token}")]
        public IActionResult GetBase64(string token)
        {
            if (TempImageStore.TryGet(token, out var data, out var contentType))
            {
                var base64 = "data:" + contentType + ";base64," + Convert.ToBase64String(data);
                return Ok(base64);
            }
            return NotFound();
        }
    }
}
