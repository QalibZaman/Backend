using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Models;

namespace NettoHealth.Infrastructure.Services;

public class IpInfoService(HttpClient httpClient, ILogger<IpInfoService> logger) : IIpInfoService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Location?> GetIpInfoAsync(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
        {
            logger.LogWarning("Cannot parse IP address '{Ip}'", ipAddress);
            return null;
        }

        if (ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();

        // For local and internal addresses, an external API is useless
        if (IsNonPublic(ip))
            return null;

        try
        {
            var response = await httpClient.GetAsync(ip.ToString());

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("ipinfo returned {StatusCode} for {Ip}", response.StatusCode, ip);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Location>(content, JsonOptions);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogError(ex, "Failed to get location for {Ip}", ip);
            return null;
        }
    }

    private static bool IsNonPublic(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
            return true;

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            return ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6UniqueLocal;

        var b = ip.GetAddressBytes();
        return b[0] == 10                                   // 10.0.0.0/8
            || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)    // 172.16.0.0/12
            || (b[0] == 192 && b[1] == 168)                 // 192.168.0.0/16
            || (b[0] == 169 && b[1] == 254)                 // link-local
            || (b[0] == 100 && b[1] >= 64 && b[1] <= 127);  // CGNAT 100.64.0.0/10
    }
}