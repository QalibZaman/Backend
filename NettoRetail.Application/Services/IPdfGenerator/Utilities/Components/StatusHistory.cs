using NettoRetail.Application.Models.Order;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal class StatusHistoryComponent(PrintOrderDetailsModel model) : IComponent
{
    public void Compose(IContainer container)
    {
        container
            .PaddingTop(Converters.ConvertPixelsToPoints(24))
            .Border(UserDefaults.DefaultBorderWidth)
            .BorderColor(UserColors.BorderColor)
            .CornerRadius(UserDefaults.DefaultBorderRadius)
            .Table((table) =>
            {
                table.ColumnsDefinition((columns) =>
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            columns.RelativeColumn();
                        }
                    });

                table.Header((header) =>
                {
                    string[] headers = ["Date", "Status", "Author", "Comment"];

                    for (int i = 0; i < headers.Length; i++)
                    {
                        header.Cell()
                            .Background(UserColors.PurpleGray)
                            .Element((text) =>
                            {
                                text
                                .Padding(Converters.ConvertPixelsToPoints(10))
                                .Text(headers[i])
                                .FontColor(UserColors.PrimaryGray)
                                .SemiBold();
                            });
                    }
                });
                var orderStatusHistory = model.StatusHistory;

                foreach (var statusHistory in orderStatusHistory)
                {
                    var updatedBy = statusHistory.UpdatedBy;
                    var author = updatedBy == null
                        ? "Error"
                        : model.CustomerId == updatedBy.Id
                            ? "Customer"
                            : string.Join(" ", updatedBy.FirstName, updatedBy.LastName);

                    string[] data = [
                        statusHistory.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                            statusHistory.NewStatus.ToString(),
                            author,
                            statusHistory.Comment ?? "-"
                    ];

                    foreach (var str in data)
                    {
                        table.Cell()
                            .Element((text) =>
                            {
                                text.Padding(Converters.ConvertPixelsToPoints(10))
                                    .Text(str)
                                    .SemiBold();
                            });
                    }
                }
            });
    }
}