using AggregationAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AggregationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AggragatedController(IAggregationService aggregationService) : ControllerBase
    {
        private readonly IAggregationService _aggregationService = aggregationService;

        [HttpGet("aggregate")]
        public async Task<IActionResult> GetAggregatedData([FromQuery] string city, [FromQuery] string title,  [FromQuery] string region)
        {
            var data = await _aggregationService.GetAggregatedDataAsync(city,title,  region);
            return Ok(data);
        }
    }

}

