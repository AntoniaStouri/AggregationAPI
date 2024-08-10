using AggregationAPI.Models;

namespace AggregationAPI.Services
{
    public class AggregationService
    {
        private readonly WeatherService _weatherService;
        private readonly NewsService _newsService;
        private readonly CountriesService _countriesService;

        public AggregationService(WeatherService weatherService, NewsService newsService, CountriesService countriesService)
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
