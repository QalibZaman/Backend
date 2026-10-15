using System.Net;
using Microsoft.Extensions.Logging;
using NettoHealth.Infrastructure.Services;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.InfrastructureTests
{
    public class IpInfoServiceTests
    {
        private readonly ILogger<IpInfoService> _logger = Substitute.For<ILogger<IpInfoService>>();

        [Fact]
        public async Task GetIpInfoAsync_ShouldMapCityAndCountry_WhenResponseIsValid()
        {
            var handler = new StubHandler(_ => Json("""{"ip":"8.8.8.8","city":"Mountain View","country":"US"}"""));
            var service = CreateService(handler);

            var result = await service.GetIpInfoAsync("8.8.8.8");

            Assert.NotNull(result);
            Assert.Equal("Mountain View", result.City);
            Assert.Equal("US", result.Country);
            Assert.Equal("https://ipinfo.io/8.8.8.8", handler.Requests.Single().RequestUri!.ToString());
        }

        [Fact]
        public async Task GetIpInfoAsync_ShouldUseIpv4_WhenAddressIsIpv4MappedToIpv6()
        {
            var handler = new StubHandler(_ => Json("""{"city":"Mountain View","country":"US"}"""));
            var service = CreateService(handler);

            await service.GetIpInfoAsync("::ffff:8.8.8.8");

            Assert.Equal("https://ipinfo.io/8.8.8.8", handler.Requests.Single().RequestUri!.ToString());
        }

        [Theory]
        [InlineData("100.100.0.15")]
        [InlineData("10.0.0.1")]
        [InlineData("172.16.0.1")]
        [InlineData("192.168.1.1")]
        [InlineData("127.0.0.1")]
        [InlineData("::1")]
        public async Task GetIpInfoAsync_ShouldReturnNullWithoutRequest_WhenIpIsNotPublic(string ip)
        {
            var handler = new StubHandler(_ => Json("{}"));
            var service = CreateService(handler);

            var result = await service.GetIpInfoAsync(ip);

            Assert.Null(result);
            Assert.Empty(handler.Requests);
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-an-ip")]
        public async Task GetIpInfoAsync_ShouldReturnNullWithoutRequest_WhenIpIsInvalid(string ip)
        {
            var handler = new StubHandler(_ => Json("{}"));
            var service = CreateService(handler);

            var result = await service.GetIpInfoAsync(ip);

            Assert.Null(result);
            Assert.Empty(handler.Requests);
        }

        [Theory]
        [InlineData(HttpStatusCode.TooManyRequests)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task GetIpInfoAsync_ShouldReturnNull_WhenProviderReturnsErrorStatus(HttpStatusCode status)
        {
            var handler = new StubHandler(_ => new HttpResponseMessage(status));
            var service = CreateService(handler);

            var result = await service.GetIpInfoAsync("8.8.8.8");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetIpInfoAsync_ShouldReturnNull_WhenResponseIsInvalidJson()
        {
            var handler = new StubHandler(_ => Json("<html>not json</html>"));
            var service = CreateService(handler);

            var result = await service.GetIpInfoAsync("8.8.8.8");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetIpInfoAsync_ShouldReturnNull_WhenRequestFails()
        {
            var handler = new StubHandler(_ => throw new HttpRequestException("Network error"));
            var service = CreateService(handler);

            var result = await service.GetIpInfoAsync("8.8.8.8");

            Assert.Null(result);
        }

        private IpInfoService CreateService(StubHandler handler)
        {
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://ipinfo.io/") };
            return new IpInfoService(client, _logger);
        }

        private static HttpResponseMessage Json(string body) =>
            new(HttpStatusCode.OK) { Content = new StringContent(body) };

        private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
        {
            public List<HttpRequestMessage> Requests { get; } = [];

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                return Task.FromResult(responder(request));
            }
        }
    }
}