using AggregationAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AggregationAPI.Test
{
    public class CountriesTests
    {
        private readonly CountriesService _countriesService;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly IMemoryCache _statisticsCacheMock;

        public CountriesTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            var memoryCacheOptions = new MemoryCacheOptions();
            _memoryCache = new MemoryCache(memoryCacheOptions);

            _statisticsCacheMock = new MemoryCache(memoryCacheOptions);


            _countriesService = new CountriesService(
                _httpClient,
                _memoryCache,
                _statisticsCacheMock);
        }

        [Fact]
        public async Task GetCountriesAsync_ReturnCountries()
        {
            var region = "Europe";
            var countriesApiResponse = @"[ {""name"": { ""common"": ""Greece"" },
               ""region"":""Europe"",
               ""population"":10724599},
              {""name"": { ""common"": ""France"" },
               ""region"":""Europe"",
               ""population"":67081000} ]";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"https://restcountries.com/v3.1/region/{region}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(countriesApiResponse)
                });

            var countries = await _countriesService.GetCountriesAsync(region);

            Assert.NotNull(countries);
            Assert.Equal("Greece", countries[0].Name);
            Assert.Equal("Europe", countries[0].Region);
            Assert.Equal(10724599, countries[0].Population);
            Assert.Equal("France", countries[1].Name);
            Assert.Equal("Europe", countries[1].Region);
            Assert.Equal(67081000, countries[1].Population);
        }

        [Fact]
        public async Task GetCountriesAsync_ExceptionError()
        {
            var region = "InvalidRegion";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"https://restcountries.com/v3.1/region/{region}")),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Request failed"));

            await Assert.ThrowsAsync<Exception>(() => _countriesService.GetCountriesAsync(region));
        }


    }
}
