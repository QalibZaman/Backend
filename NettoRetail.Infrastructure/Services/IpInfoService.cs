using System.Text.Json;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;

namespace NettoHealth.Infrastructure.Services;

public class IpInfoService : IIpInfoService
{
    private readonly HttpClient _httpClient;

    public IpInfoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Location?> GetIpInfoAsync(string ipAddress)
    {
        var response = await _httpClient.GetAsync($"{ipAddress}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var location = JsonSerializer.Deserialize<Location>(content);

        return location;
    }
}
