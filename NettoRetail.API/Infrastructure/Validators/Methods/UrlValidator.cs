namespace NettoRetail.API.Infrastructure.Validators.Methods;

public class UrlValidator
{
    public static bool BeAValidHttpOrHttpsDomain(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        if (!uri.Host.Contains('.') || uri.Host.EndsWith('.'))
            return false;

        return true;
    }
}
