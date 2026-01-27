namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal static class LineHeightComputer
{
    public static float ComputeFromPixels(float fontSizePx, float lineHeightPx)
    {
        if (fontSizePx <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fontSizePx), "Font size in pixels must be greater than zero.");
        }

        return lineHeightPx / fontSizePx;
    }
}