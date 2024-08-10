namespace AggregationAPI.Models
{
    public class AggregatedData
    {
        public Weather WeatherData { get; set; }
        public IEnumerable<News> NewsData { get; set; }
        public IEnumerable<Country> CountryData { get; set; }
            
    }
}
