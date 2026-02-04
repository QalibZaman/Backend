namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal static class UserDefaults
{
    public static readonly float DefaultFontSize = Converters.ConvertPixelsToPoints(16);
    public static readonly float DefaultBorderWidth = Converters.ConvertPixelsToPoints(1);
    public static readonly float DefaultBorderRadius = Converters.ConvertPixelsToPoints(8);
}