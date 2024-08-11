using AggregationAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AggregationAPI.Services
{
    public interface IWeatherService
    {
        Task<Weather> GetWeatherAsync(string city);
    }

    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IMemoryCache _cache;

        public WeatherService(HttpClient httpClient,IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _apiKey = configuration["WeatherAPIKey"];
            _cache = cache;
        }

        public async Task<Weather> GetWeatherAsync(string city)
        {
            if (_cache.TryGetValue($"weather_{city}", out Weather cachedWeather))
            {
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
                return weather;
            }
            catch(HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

