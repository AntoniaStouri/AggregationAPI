using AggregationAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text.Json;
using static AggregationAPI.Models.Statistics;

namespace AggregationAPI.Services
{
    public interface INewsService
    {
        Task<List<News>> GetNewsAsync(string topic);
        AggregatedStatistics GetStatistics();
    }
    public class NewsService : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string _apiKey;
        private readonly string _statisticsCacheKey = "NewsStatistics";
        private readonly IMemoryCache _statisticsCache;

        public NewsService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache, IMemoryCache statisticsCache)
        {
            _httpClient = httpClient;
            _apiKey = configuration["NewsAPIKey"];
            _cache = cache;
            _statisticsCache = statisticsCache;
        }

        public async Task<List<News>> GetNewsAsync(string title)
        {
            var watch = new Stopwatch();
            watch.Start();
            if (_cache.TryGetValue($"news_{title}", out List<News> cachedNews))
            {
                watch.Stop();
                RecordStatistics(watch.ElapsedMilliseconds);
                return cachedNews;
            }

            try
            {
                var response = await _httpClient.GetStringAsync($"https://newsapi.org/v2/everything?q={title}&apiKey={_apiKey}");
                var jsonDoc = JsonDocument.Parse(response);

                var news = new List<News>();

                var articles = jsonDoc.RootElement.GetProperty("articles").EnumerateArray();
                foreach (var article in articles)
                {
                    news.Add(new News
                    {
                        Title = article.GetProperty("title").GetString(),
                        Description = article.GetProperty("description").GetString(),
                        Author = article.GetProperty("author").GetString(),
                        PublishedAt = article.GetProperty("publishedAt").GetDateTime()
                    });
                }
                _cache.Set($"news_{title}", news, TimeSpan.FromMinutes(1));
                watch.Stop();
                RecordStatistics(watch.ElapsedMilliseconds);

                return news;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"An internal error occurred while processing the request,{ex.Message}");
            }
        }

        private void RecordStatistics(long elapsedMilliseconds)
        {
            var statistics = _statisticsCache.GetOrCreate(_statisticsCacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(3);
                return new AggregatedStatistics();
            });

            statistics.NewsStatistics.RecordRequest(elapsedMilliseconds);
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
