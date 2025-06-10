using Microsoft.AspNetCore.Mvc;
using DataAccessLibrary.Services;

namespace VoiceLauncherBlazor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestSearchController : ControllerBase
    {
        private readonly ICustomIntelliSenseDataService _customIntelliSenseDataService;

        public TestSearchController(ICustomIntelliSenseDataService customIntelliSenseDataService)
        {
            _customIntelliSenseDataService = customIntelliSenseDataService;
        }

        [HttpGet("search/{term}")]
        public async Task<IActionResult> TestSearch(string term)
        {
            try
            {
                Console.WriteLine($"=== TEST SEARCH API CALLED WITH TERM: '{term}' ===");
                var results = await _customIntelliSenseDataService.SearchCustomIntelliSensesAsync(term);
                Console.WriteLine($"=== TEST SEARCH API RETURNED {results.Count()} RESULTS ===");
                
                return Ok(new 
                { 
                    SearchTerm = term,
                    ResultCount = results.Count(),
                    Results = results.Take(5).Select(r => new { 
                        r.Id, 
                        r.DisplayValue, 
                        SendKeysPreview = r.SendKeysValue?.Length > 50 ? r.SendKeysValue.Substring(0, 50) + "..." : r.SendKeysValue,
                        r.CommandType,
                        r.DeliveryType 
                    })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== TEST SEARCH API ERROR: {ex.Message} ===");
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
