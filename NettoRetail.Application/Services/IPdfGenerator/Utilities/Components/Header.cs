using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal record struct HeaderConfiguration(
    string Text,
    float FontSizePx,
    float PaddingBottomPx);
internal class HeaderComponent(HeaderConfiguration configs) : IComponent
{
    private readonly HeaderConfiguration _configs = configs;
    public void Compose(IContainer container)
    {
        container
            .PaddingBottom(Converters.ConvertPixelsToPoints(_configs.PaddingBottomPx))
            .Text(_configs.Text)
            .FontSize(Converters.ConvertPixelsToPoints(_configs.FontSizePx))
            .LineHeight(LineHeightComputer.ComputeFromPixels(24, 32))
            .SemiBold();
    }
}