using AggregationAPI.Models;
using System.Text.Json;

namespace AggregationAPI.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService(HttpClient httpClient,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["WeatherAPIKey"];
        }

        public async Task<Weather> GetWeatherAsync(string city)
        {
            var response = await _httpClient.GetStringAsync($"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}");
            var jsonDoc = JsonDocument.Parse(response);

            var weather = new Weather
            {
                City = jsonDoc.RootElement.GetProperty("name").GetString(),
                Description = jsonDoc.RootElement.GetProperty("weather")[0].GetProperty("description").GetString(),
                Temperature = jsonDoc.RootElement.GetProperty("main").GetProperty("temp").GetDouble()
            };

            return weather;
        }
    }
}

