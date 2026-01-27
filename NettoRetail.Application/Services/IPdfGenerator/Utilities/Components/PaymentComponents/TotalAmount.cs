using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal class TotalAmountComponent(int totalProducts, int totalQuantity) : IComponent
{
    public void Compose(IContainer container)
    {
        container
            .PaddingTop(Converters.ConvertPixelsToPoints(24))
            .Row((row) =>
            {
                row.RelativeItem()
                    .Text($"Total items: {totalProducts}");

                row.RelativeItem()
                    .Text($"Total quantity: {totalQuantity}");
            });
    }
}