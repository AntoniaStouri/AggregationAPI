using AggregationAPI.Models;
using static AggregationAPI.Models.Statistics;

namespace AggregationAPI.Services
{
    public interface IAggregationService
    {
        Task<AggregatedData> GetAggregatedDataAsync(string city, string title, string region, string sortBy, string filterBy);
        Task<AggregatedStatistics> GetStatisticsAsync();
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

        public async Task<AggregatedData> GetAggregatedDataAsync(string city, string title, string region, string? sortBy, string? filterBy)
        {
            var weather = _weatherService.GetWeatherAsync(city);
            var news = _newsService.GetNewsAsync(title);
            var countries = _countriesService.GetCountriesAsync(region);

            await Task.WhenAll(weather, news, countries);

            return new AggregatedData
            {
                WeatherData = weather.Result,
                NewsData = news.Result,
                CountryData = FilterAndSortCountries(countries.Result,filterBy,sortBy)
            };
        }

        private List<Country> FilterAndSortCountries(List<Country> country, string? filterBy, string? sortBy)
        {
            if (!string.IsNullOrEmpty(filterBy))
            {
                country = country.Where(n => n.Name.Equals(filterBy, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                country = sortBy.ToLower() switch
                {
                    "name" => country.OrderBy(n => n.Name).ToList(),
                    "population" => country.OrderBy(n => n.Population).ToList(),
                    _ => country
                };
            }

            return country;
        }

        public async Task<AggregatedStatistics> GetStatisticsAsync()
        {
            var weatherStats = _weatherService.GetStatistics();
            var newsStats = _newsService.GetStatistics();
            var countriesStats = _countriesService.GetStatistics();

            return new AggregatedStatistics
            {
                WeatherStatistics = weatherStats.WeatherStatistics,
                NewsStatistics = newsStats.NewsStatistics,
                CountriesStatistics = countriesStats.CountriesStatistics
            };
        }
    }
}
