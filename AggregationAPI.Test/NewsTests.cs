using AggregationAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AggregationAPI.Test
{
    public class NewsTests
    {
        private readonly NewsService _newsService;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly IMemoryCache _statisticsCacheMock;

        public NewsTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            var memoryCacheOptions = new MemoryCacheOptions();
            _memoryCache = new MemoryCache(memoryCacheOptions);

            _statisticsCacheMock = new MemoryCache(memoryCacheOptions);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["NewsAPIKey"]).Returns("api_key");

            _newsService = new NewsService(
                _httpClient,
                configurationMock.Object,
                _memoryCache,
                _statisticsCacheMock);
        }

        [Fact]
        public async Task GetNewsAsync_ReturnNews()
        {
            var title = "title1";
            var newsApiResponse = @"{ ""articles"": [ {
                ""title"": ""Title 1"",
                ""description"": ""Description 1"",
                ""author"": ""Author 1"",
                ""publishedAt"": ""2024-07-14T11:34:00Z""
               },
               { ""title"": ""Title 2"", 
                 ""description"": ""Description 2"",
                 ""author"": ""Author 2"",
                 ""publishedAt"": ""2024-07-14T11:34:00Z""
               } ] }";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"https://newsapi.org/v2/everything?q={title}&apiKey=api_key")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(newsApiResponse)
                });

            var news = await _newsService.GetNewsAsync(title);

            Assert.NotNull(news);
            Assert.Equal("Title 1", news[0].Title);
            Assert.Equal("Description 1", news[0].Description);
            Assert.Equal("Author 1", news[0].Author);
            Assert.Equal("Title 2", news[1].Title);
            Assert.Equal("Description 2", news[1].Description);
            Assert.Equal("Author 2", news[1].Author);

        }

        [Fact]
        public async Task GetNewsAsync_ExceptionError()
        {
            var title = "InvalidTitle";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"https://newsapi.org/v2/everything?q={title}&apiKey=api_key")),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Request failed"));

            await Assert.ThrowsAsync<Exception>(() => _newsService.GetNewsAsync(title));
        }


    }
}

