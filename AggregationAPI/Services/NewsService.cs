using AggregationAPI.Models;
using System.Text.Json;

namespace AggregationAPI.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;

        public NewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<News>> GetNewsAsync(string topic)
        {
            
            var encodedTopic = Uri.EscapeDataString(topic);
            var url = $"https://newsapi.org/v2/everything?q=tesla&from=2024-07-10&sortBy=publishedAt&apiKey=6456d9e1bbd94ab19b7e07af6f30c39c";

            HttpResponseMessage response;
            
                response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
            

            var responseString = await response.Content.ReadAsStringAsync();

            var news = new List<News>();
            using (var jsonDoc = JsonDocument.Parse(responseString))
            {
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
            }

            return news;
        }
    }
}
