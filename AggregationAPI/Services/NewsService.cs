using AggregationAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace AggregationAPI.Services
{
    public interface INewsService
    {
        Task<List<News>> GetNewsAsync(string topic);
    }
    public class NewsService : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string _apiKey;

        public NewsService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _apiKey = configuration["NewsAPIKey"];
            _cache = cache;
        }

        public async Task<List<News>> GetNewsAsync(string title)
        {
            if (_cache.TryGetValue($"news_{title}", out List<News> cachedNews))
            {
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

                return news;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
