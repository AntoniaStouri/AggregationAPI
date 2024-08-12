using AggregationAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text.Json;
using static AggregationAPI.Models.Statistics;

namespace AggregationAPI.Services
{
    public interface IWeatherService
    {
        Task<Weather> GetWeatherAsync(string city);
        AggregatedStatistics GetStatistics();
    }

    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IMemoryCache _cache;
        private readonly string _statisticsCacheKey = "WeatherStatistics";
        private readonly IMemoryCache _statisticsCache;

        public WeatherService(HttpClient httpClient,IConfiguration configuration, IMemoryCache cache, IMemoryCache statisticsCache)
        {
            _httpClient = httpClient;
            _apiKey = configuration["WeatherAPIKey"];
            _cache = cache;
            _statisticsCache = statisticsCache;
        }

        public async Task<Weather> GetWeatherAsync(string city)
        {;
            var watch = new Stopwatch();
            watch.Start();
            if (_cache.TryGetValue($"weather_{city}", out Weather cachedWeather))
            {
                watch.Stop();
                RecordStatistics(watch.ElapsedMilliseconds);
                return cachedWeather;
            }

            try
            {
                var response = await _httpClient.GetStringAsync($"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}");
                var jsonDoc = JsonDocument.Parse(response);

                var weather = new Weather
                {
                    City = jsonDoc.RootElement.GetProperty("name").GetString(),
                    Description = jsonDoc.RootElement.GetProperty("weather")[0].GetProperty("description").GetString(),
                    Temperature = jsonDoc.RootElement.GetProperty("main").GetProperty("temp").GetDouble()
                };
                _cache.Set($"weather_{city}", weather, TimeSpan.FromMinutes(10));
                watch.Stop();
                RecordStatistics(watch.ElapsedMilliseconds);

                return weather;
            }
            catch(HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private void RecordStatistics(long elapsedMilliseconds)
        {
            var statistics = _statisticsCache.GetOrCreate(_statisticsCacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(3); 
                return new AggregatedStatistics();
            });

            statistics.WeatherStatistics.RecordRequest(elapsedMilliseconds);
            _statisticsCache.Set(_statisticsCacheKey, statistics, TimeSpan.FromMinutes(3));
        }

        public AggregatedStatistics GetStatistics()
        {
            return _statisticsCache.TryGetValue(_statisticsCacheKey, out AggregatedStatistics stats)
                ? stats
                : new AggregatedStatistics();
        }
    }
}

