using AggregationAPI.Models;
using System.Text.Json;

namespace AggregationAPI.Services
{
    public class CountriesService(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient;

        public async Task<List<Country>> GetCountriesAsync(string region)
        {
            var response = await _httpClient.GetStringAsync($"https://restcountries.com/v3.1/region/{region}");

            using var jsonDoc = JsonDocument.Parse(response);

            var countries = new List<Country>();

            foreach (var countryElement in jsonDoc.RootElement.EnumerateArray())
            {
                countries.Add(new Country
                {
                    Name = countryElement.GetProperty("name").GetProperty("common").GetString(),
                    Region = countryElement.GetProperty("region").GetString(),
                    Population = countryElement.GetProperty("population").GetInt32()
                });
            }

            return countries;
        }
    }
}

