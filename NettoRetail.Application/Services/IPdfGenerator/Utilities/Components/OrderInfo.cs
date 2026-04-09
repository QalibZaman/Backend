using NettoRetail.Application.Models.Order;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal class OrderInfoComponent(PrintOrderDetailsModel model) : IComponent
{
    public void Compose(IContainer container)
    {
        float fontSize = Converters.ConvertPixelsToPoints(16);
        float lineHeight = LineHeightComputer.ComputeFromPixels(16, 24);
        Color headerFontColor = UserColors.PrimaryGray;
        Color infoFontColor = UserColors.PrimaryBlack;

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            foreach (var (left, right) in GetRows())
            {
                table.Cell().Column(column =>
                {
                    column.Item()
                        .PaddingTop(Converters.ConvertPixelsToPoints(24))
                        .Text(left.Key)
                        .FontColor(headerFontColor)
                        .FontSize(fontSize)
                        .LineHeight(lineHeight)
                        .SemiBold();

                    column.Item()
                        .PaddingTop(Converters.ConvertPixelsToPoints(4))
                        .Text(left.Value)
                        .FontColor(infoFontColor)
                        .FontSize(fontSize)
                        .LineHeight(lineHeight);
                });

                table.Cell().Column(column =>
                {
                    column.Item()
                        .PaddingTop(Converters.ConvertPixelsToPoints(24))
                        .Text(right.Key)
                        .FontColor(headerFontColor)
                        .FontSize(fontSize)
                        .LineHeight(lineHeight)
                        .SemiBold();

                    column.Item()
                        .PaddingTop(Converters.ConvertPixelsToPoints(4))
                        .Text(right.Value)
                        .FontColor(infoFontColor)
                        .FontSize(fontSize)
                        .LineHeight(lineHeight);
                });
            }
        });
    }

    private List<(KeyValuePair<string, string> Left, KeyValuePair<string, string> Right)> GetRows()
    {
        Dictionary<string, string> leftColumnContent = new()
        {
            { "ID", model.Id.ToString() },
            { "Customer", $"{model.CustomerId}\n{model.CustomerFirstName} {model.CustomerLastName}\n{model.CustomerEmail}" },
            { "Status", model.OrderStatus },
        };

        Dictionary<string, string> rightColumnContent = new()
        {
            { "Created", model.CreationDate.ToString("yyyy-MM-dd HH:mm:ss") },
            { "Delivery type", model.DeliveryType },
            { "Creator", model.Creator }
        };

        return leftColumnContent
            .Zip(rightColumnContent, (left, right) => (left, right))
            .ToList();
    }
}