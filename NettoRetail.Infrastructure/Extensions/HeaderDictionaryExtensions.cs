using Microsoft.AspNetCore.Http;

namespace NettoRetail.Infrastructure.Extensions;

public static class HeaderDictionaryExtensions
{
    public static string GetUserAgent(this IHeaderDictionary headers)
    {
        return headers.TryGetValue("User-Agent", out var userAgent)
            ? userAgent.ToString()
            : string.Empty;
    }
}