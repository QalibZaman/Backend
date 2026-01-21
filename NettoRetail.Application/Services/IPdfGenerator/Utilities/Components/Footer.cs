using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal class FooterComponent(string title, string value) : IComponent
{
    public void Compose(IContainer container)
    {
        container
        .PaddingBottom(Converters.ConvertPixelsToPoints(24))
            .Row((row) =>
            {
                row.RelativeItem()
                    .AlignLeft()
                    .Text(title)
                    .Bold();

                row.RelativeItem()
                    .AlignRight()
                    .Text(value)
                    .Bold();
            });
    }
}