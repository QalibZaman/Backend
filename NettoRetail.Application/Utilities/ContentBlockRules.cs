using System.Net;
using System.Text.RegularExpressions;
using Ganss.Xss;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Utilities;

public static partial class ContentBlockRules
{
    public const int MaxImagesPerBlock = 10;
    public const int MinImageSizeInBytes = 1_024;
    public const int MaxImageSizeInBytes = 5 * 1024 * 1024;

    private static readonly HtmlSanitizer Sanitizer = new();

    public static string Sanitize(string? content)
    {
        return string.IsNullOrWhiteSpace(content)
            ? string.Empty
            : Sanitizer.Sanitize(content);
    }

    public static bool HasMeaningfulContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        if (ImageTagRegex().IsMatch(content))
        {
            return true;
        }

        var textContent = HtmlTagRegex().Replace(content, string.Empty);
        return !string.IsNullOrWhiteSpace(WebUtility.HtmlDecode(textContent));
    }

    public static bool ShouldBeVisible(bool isVisible, string? content)
    {
        return isVisible && HasMeaningfulContent(content);
    }

    public static string? ValidateImages(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var images = ImageTagRegex().Matches(content);
        if (images.Count > MaxImagesPerBlock)
        {
            return ValidationErrorMessages.Content.ImageLimitExceededMessage;
        }

        foreach (Match image in images)
        {
            var source = image.Groups["src"].Value;
            if (!source.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var validationError = ValidateDataUrlImage(source);
            if (validationError != null)
            {
                return validationError;
            }
        }

        return null;
    }

    private static string? ValidateDataUrlImage(string dataUrl)
    {
        var match = DataUrlImageRegex().Match(dataUrl);
        if (!match.Success)
        {
            return ValidationErrorMessages.Content.InvalidImageFormatMessage;
        }

        var imageType = match.Groups["type"].Value.ToLowerInvariant();
        if (imageType is not ("jpg" or "jpeg" or "png" or "gif"))
        {
            return ValidationErrorMessages.Content.InvalidImageFormatMessage;
        }

        try
        {
            var bytes = Convert.FromBase64String(match.Groups["data"].Value);
            if (bytes.Length < MinImageSizeInBytes)
            {
                return ValidationErrorMessages.Content.ImageSizeTooSmallMessage;
            }

            if (bytes.Length > MaxImageSizeInBytes)
            {
                return ValidationErrorMessages.Content.ImageSizeTooLargeMessage;
            }
        }
        catch (FormatException)
        {
            return ValidationErrorMessages.Content.InvalidImageFormatMessage;
        }

        return null;
    }

    [GeneratedRegex("<img\\b[^>]*?src\\s*=\\s*[\"'](?<src>[^\"']+)[\"'][^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ImageTagRegex();

    [GeneratedRegex("data:image/(?<type>[^;]+);base64,(?<data>.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex DataUrlImageRegex();

    [GeneratedRegex("<.*?>", RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();
}
