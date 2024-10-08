﻿using AggregationAPI.Models;
using AggregationAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AggregationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AggragatedController(IAggregationService aggregationService) : ControllerBase
    {
        private readonly IAggregationService _aggregationService = aggregationService;

        [HttpGet("aggregatedData")]
        public async Task<IActionResult> GetAggregatedData([FromQuery] string city, [FromQuery] string title, [FromQuery] string region, [FromQuery] string? sortBy, [FromQuery] string? filterBy)
        {
            var data = await _aggregationService.GetAggregatedDataAsync(city, title, region, sortBy, filterBy);
            return Ok(data);
        }

        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            var statistics = _aggregationService.GetStatisticsAsync();
            return Ok(statistics);
        }

    }
}

