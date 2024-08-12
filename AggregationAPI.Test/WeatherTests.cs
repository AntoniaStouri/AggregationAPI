using AggregationAPI.Models;
using AggregationAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace AggregationAPI.Tests
{
    public class WeatherTests
    {
        private readonly WeatherService _weatherService;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly IMemoryCache _statisticsCacheMock;

        public WeatherTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            var memoryCacheOptions = new MemoryCacheOptions();
            _memoryCache = new MemoryCache(memoryCacheOptions);

            _statisticsCacheMock = new MemoryCache(memoryCacheOptions);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["WeatherAPIKey"]).Returns("api_key");

            _weatherService = new WeatherService(
                _httpClient,
                configurationMock.Object,
                _memoryCache,
                _statisticsCacheMock);
        }

        [Fact]
        public async Task GetWeatherAsync_ReturnWeather()
        {
            var city = "Athens";
            var weatherApiResponse = @"{
                ""name"": ""Athens"",
                ""weather"": [{ ""description"": ""Clear"" }],
                ""main"": { ""temp"": 300.49 }
            }";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://api.openweathermap.org/data/2.5/weather?q={city}&appid=api_key")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(weatherApiResponse)
                });

            var weather = await _weatherService.GetWeatherAsync(city);

            Assert.NotNull(weather);
            Assert.Equal("Athens", weather.City);
            Assert.Equal("Clear", weather.Description);
            Assert.Equal(300.49, weather.Temperature);
        }

        [Fact]
        public async Task GetWeatherAsync_ExceptionError()
        {
            var city = "InvalidCity";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"http://api.openweathermap.org/data/2.5/weather?q={city}&appid=api_key")),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Request failed"));

            await Assert.ThrowsAsync<Exception>(() => _weatherService.GetWeatherAsync(city));
        }


    }
}
