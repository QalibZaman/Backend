using System.Text.Json;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;

namespace NettoHealth.Infrastructure.Services;

public class IpInfoService(HttpClient httpClient) : IIpInfoService
{
    public async Task<Location?> GetIpInfoAsync(string ipAddress)
    {
        var response = await httpClient.GetAsync($"{ipAddress}");
        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();
        var location = JsonSerializer.Deserialize<Location>(content);

        return location;
    }
}
