using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal static class PageExtensions
{
    private static readonly float _verticalPagePaddingPx = 34;
    private static readonly float _horizontalPagePaddingPx = 32;
    public static void ConfigureSettingsForOrderPrint(this PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.PageColor(UserColors.PrimaryWhite);
        page.MarginVertical(Converters.ConvertPixelsToPoints(_verticalPagePaddingPx));
        page.MarginHorizontal(Converters.ConvertPixelsToPoints(_horizontalPagePaddingPx));
        page.DefaultTextStyle(
            TextStyle.Default
                .FontFamily("Inter")
                .FontColor(UserColors.PrimaryBlack)
                .FontSize(UserDefaults.DefaultFontSize)
                .LineHeight(LineHeightComputer.ComputeFromPixels(16, 24))
        );
    }
}