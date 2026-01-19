using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal record struct SectionHeaderConfiguration(
    string Text,
    float HorizontalLineWidthPx,
    float PaddingTopPx = 0);

internal class SectionHeaderComponent(SectionHeaderConfiguration configs) : IComponent
{
    private readonly SectionHeaderConfiguration _configs = configs;
    public void Compose(IContainer container)
    {
        container.Column((column) =>
        {
            column.Item()
                .PaddingTop(Converters.ConvertPixelsToPoints(_configs.PaddingTopPx))
                .PaddingBottom(Converters.ConvertPixelsToPoints(8))
                .Text(_configs.Text)
                .FontSize(Converters.ConvertPixelsToPoints(14))
                .LineHeight(LineHeightComputer.ComputeFromPixels(14, 22));

            column.Item()
                .Height(Converters.ConvertPixelsToPoints(2))
                .Width(Converters.ConvertPixelsToPoints(_configs.HorizontalLineWidthPx))
                .BackgroundLinearGradient(
                    0,
                    UserColors.SectionHeaderGradientColors
                );
        });
    }
}