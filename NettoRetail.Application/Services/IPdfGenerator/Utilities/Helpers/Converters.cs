namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal static class Converters
{
    public static float ConvertPixelsToPoints(float px) => px * 0.75f;
}