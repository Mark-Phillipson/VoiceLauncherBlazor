using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace VoiceLauncher.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileExistsController : ControllerBase
    {
        [HttpGet("exists")]
        public IActionResult Exists([FromQuery] string folder, [FromQuery] string filename)
        {
            if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(filename))
                return BadRequest();
            var root = Directory.GetCurrentDirectory();
            var path = Path.Combine(root, folder, filename);
            bool exists = System.IO.File.Exists(path);
            return Ok(new { exists });
        }
    }
}
