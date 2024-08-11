using AggregationAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AggregationAPI.Services
{
    public interface ICountriesService
    {
        Task<List<Country>> GetCountriesAsync(string region);
    }
    public class CountriesService : ICountriesService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public CountriesService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }


        public async Task<List<Country>> GetCountriesAsync(string region)
        {
            if (_cache.TryGetValue($"countries_{region}", out List<Country> cachedCountries))
            {
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
                return countries;
            }
            catch(HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

