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

        public NewsService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<List<News>> GetNewsAsync(string topic)
        {
            if (_cache.TryGetValue($"news_{topic}", out List<News> cachedNews))
            {
                return cachedNews;
            }

            try
            {
                var response = await _httpClient.GetStringAsync($"https://newsapi.org/v2/everything?q=tesla&apiKey=6456d9e1bbd94ab19b7e07af6f30c39c");
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
                _cache.Set($"news_{topic}", news, TimeSpan.FromMinutes(1));

                return news;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
