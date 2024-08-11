using AggregationAPI.Models;

namespace AggregationAPI.Services
{
    public interface IAggregationService
    {
        Task<AggregatedData> GetAggregatedDataAsync(string city, string title, string region);
    }

    public class AggregationService : IAggregationService
    {
        private readonly IWeatherService _weatherService;
        private readonly INewsService _newsService;
        private readonly ICountriesService _countriesService;

        public AggregationService(IWeatherService weatherService, INewsService newsService, ICountriesService countriesService)
        {
            _weatherService = weatherService;
            _newsService = newsService;
            _countriesService = countriesService;
        }

        public async Task<AggregatedData> GetAggregatedDataAsync(string city, string title, string region)
        {
            var weather = _weatherService.GetWeatherAsync(city);
            var news = _newsService.GetNewsAsync(title);
            var countries = _countriesService.GetCountriesAsync(region);

            await Task.WhenAll(weather, news,   countries);

            return new AggregatedData
            {
                WeatherData = weather.Result,
                NewsData = news.Result,
                CountryData = countries.Result
            };
        }
    }
}
