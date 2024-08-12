using AggregationAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text.Json;
using static AggregationAPI.Models.Statistics;

namespace AggregationAPI.Services
{
    public interface ICountriesService
    {
        Task<List<Country>> GetCountriesAsync(string region);
        AggregatedStatistics GetStatistics();
    }
    public class CountriesService : ICountriesService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string _statisticsCacheKey = "CountriesStatistics";
        private readonly IMemoryCache _statisticsCache;

        public CountriesService(HttpClient httpClient, IMemoryCache cache, IMemoryCache statisticsCache)
        {
            _httpClient = httpClient;
            _cache = cache;
            _statisticsCache = statisticsCache;
        }


        public async Task<List<Country>> GetCountriesAsync(string region)
        {
            var watch = new Stopwatch();
            watch.Start();
            if (_cache.TryGetValue($"countries_{region}", out List<Country> cachedCountries))
            {
                watch.Stop();
                RecordStatistics(watch.ElapsedMilliseconds);
                return cachedCountries;
            }

            try
            {
                var response = await _httpClient.GetStringAsync($"https://restcountries.com/v3.1/region/{region}");
               
                using var jsonDoc = JsonDocument.Parse(response);

                var countries = new List<Country>();

                foreach (var country in jsonDoc.RootElement.EnumerateArray())
                {
                    countries.Add(new Country
                    {
                        Name = country.GetProperty("name").GetProperty("common").GetString(),
                        Region = country.GetProperty("region").GetString(),
                        Population = country.GetProperty("population").GetInt32()
                    });
                }
                _cache.Set($"countries_{region}", countries, TimeSpan.FromMinutes(1));
                watch.Stop();
                RecordStatistics(watch.ElapsedMilliseconds);
                return countries;
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

            statistics.CountriesStatistics.RecordRequest(elapsedMilliseconds);
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

