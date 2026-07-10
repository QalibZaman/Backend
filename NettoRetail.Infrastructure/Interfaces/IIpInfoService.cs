using NettoRetail.Infrastructure.Models;

namespace NettoRetail.Infrastructure.Interfaces;

public interface IIpInfoService
{
    Task<Location?> GetIpInfoAsync(string ipAddress);
}