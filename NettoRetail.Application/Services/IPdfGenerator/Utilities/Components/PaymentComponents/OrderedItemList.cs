using System.Globalization;
using NettoRetail.Application.Models.Order;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal class OrderedItemListComponent(PrintOrderDetailsModel model) : IComponent
{
    public void Compose(IContainer container)
    {
        container
            .PaddingTop(Converters.ConvertPixelsToPoints(24))
            .Border(UserDefaults.DefaultBorderWidth)
            .BorderColor(UserColors.PrimaryGray)
            .Table((table) =>
            {
                table.ColumnsDefinition((columns) =>
                {
                    for (int i = 0; i < 5; i++)
                    {
                        columns.RelativeColumn();
                    }
                });

                int index = 1;
                foreach (var item in model.InventoryItems)
                {
                    table.Cell()
                        .ColumnSpan(5)
                        .PaddingTop(Converters.ConvertPixelsToPoints(32))
                        .PaddingHorizontal(Converters.ConvertPixelsToPoints(32))
                        .PaddingBottom(Converters.ConvertPixelsToPoints(10))
                        .Text($"{index}. {item.ProductName}")
                        .Bold()
                        .FontColor(UserColors.SecondaryBlack);

                    var cultureInfo = CultureInfo.GetCultureInfo("en-US");
                    (string Header, string Value)[] headerValuePairs = [
                        ("UPC", item.ProductUpc),
                        ("Availability", $"In stock ({item.AvailableAmount})"),
                        ("Each", item.Price.ToString("C", cultureInfo)),
                        ("Quantity", item.ReservedAmount.ToString()),
                        ("Total", (item.Price * item.ReservedAmount).ToString("C", cultureInfo))
                    ];

                    foreach (var pair in headerValuePairs)
                    {
                        table.Cell()
                            .PaddingHorizontal(Converters.ConvertPixelsToPoints(32))
                            .PaddingBottom(Converters.ConvertPixelsToPoints(32))
                            .Column(column =>
                            {
                                column.Item()
                                    .Text(pair.Header)
                                    .FontColor(UserColors.PrimaryGray);

                                column.Item()
                                    .Text(pair.Value)
                                    .FontColor(Colors.Black);
                            });
                    }

                    index++;
                }
            });
    }
}
