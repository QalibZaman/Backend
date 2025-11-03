using NettoRetail.Application.Models.Order;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator
{
    public class PdfGenerator : IPdfGenerator
    {
        public async Task<Stream> PrintOrderDetails(PrintOrderDetailsModel model)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var stream = new MemoryStream();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(TextStyle.Default.FontSize(10).FontFamily("Helvetica"));

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Order #{model.Id}")
                            .FontSize(13)
                            .Bold()
                            .FontColor(Colors.Black);

                        col.Item().PaddingTop(8)
                            .Text("Order info")
                            .FontSize(10)
                            .FontColor(Colors.Black);


                        col.Item().PaddingVertical(6)
                            .AlignLeft() 
                            .Element(e => e.Width(70)) 
                            .LineHorizontal(0.5f)
                            .LineColor("#7E57C2");

                        col.Item().PaddingTop(10).Grid(grid =>
                        {
                            grid.Columns(2);

                            grid.Item().Column(left =>
                            {
                                left.Item().Text("ID").Bold().FontColor(Colors.Grey.Darken1);
                                left.Item().Text($"#{model.Id}");

                                left.Item().PaddingTop(8).Text("Customer").Bold().FontColor(Colors.Grey.Darken1);
                                left.Item().PaddingTop(2).Text($"#{model.CustomerId}");
                                left.Item().PaddingTop(2).Text($"{model.CustomerFirstName} {model.CustomerLastName}");
                                left.Item().PaddingTop(2).Text(model.CustomerEmail);

                                left.Item().PaddingTop(8).Text("Status").Bold().FontColor(Colors.Grey.Darken1);
                                left.Item().PaddingTop(2).Text(model.OrderStatus);
                            });

                            grid.Item().Column(right =>
                            {
                                right.Item().Text("Created").FontColor(Colors.Grey.Darken1).Bold();
                                right.Item().PaddingTop(2).Text($"{model.CreationDate:dd.MM.yyyy}");

                                right.Item().PaddingTop(8).Text("Delivery type").FontColor(Colors.Grey.Darken1).Bold();
                                right.Item().PaddingTop(2).Text(model.DeliveryType);

                                right.Item().PaddingTop(8).Text("Creator").Bold().FontColor(Colors.Grey.Darken1);
                                right.Item().PaddingTop(2).Text(model.Creator);
                            });
                        });

                        // STATUS HISTORY TABLE
                        col.Item().PaddingTop(25)
                          .Border(1)
                          .BorderColor(Colors.Grey.Lighten2)
                          .CornerRadius(8)
                          .Table(table =>
                          {
                              table.ColumnsDefinition(columns =>
                              {
                                  columns.RelativeColumn(2);
                                  columns.RelativeColumn(3);
                                  columns.RelativeColumn(2);
                                  columns.RelativeColumn(3);
                              });

                              table.Header(header =>
                              {
                                  void HeaderCell(string text) => header.Cell().Element(c =>
                                  {
                                      c.Background(Colors.Grey.Lighten3)
                                       .PaddingVertical(6)
                                       .PaddingHorizontal(6)
                                       .AlignMiddle()
                                       .Text(text).Bold().FontColor(Colors.Grey.Darken1);
                                  });

                                  HeaderCell("Date");
                                  HeaderCell("Status");
                                  HeaderCell("Author");
                                  HeaderCell("Author");
                              });

                              col.Item().PaddingTop(30);

                              foreach (var status in model?.StatusHistory)
                              {
                                  table.Cell().Padding(6).Text($"{status.UpdatedAt:dd.MM.yyyy}");
                                  table.Cell().Padding(6).Text($"{status.OldStatus} → {status.NewStatus}");
                                  table.Cell().Padding(6).Text(model.Creator);
                                  table.Cell().Padding(6).Text(status.Comment ?? "-");
                              }
                          });
                        col.Item().PaddingTop(8)
                             .Text("Products and payment")
                             .FontSize(10)
                             .FontColor(Colors.Black);


                        col.Item().PaddingVertical(6)
                            .AlignLeft()
                            .Element(e => e.Width(105))
                            .LineHorizontal(0.5f)
                            .LineColor("#7E57C2");

                        col.Item().PaddingTop(5).PaddingRight(20).PaddingVertical(10).Row(row =>
                        {
                            row.AutoItem()
                                .Text($"Total products: {model.TotalProducts}")
                                .FontSize(10)
                                .FontColor(Colors.Black);

                            row.RelativeItem()
                                .AlignRight()
                                .Text($"Total quantity: {model.TotalQuantity}")
                                .FontSize(10)
                                .FontColor(Colors.Black);
                        });

                        // PRODUCT LIST 
                        col.Item().PaddingVertical(12)
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten3)
                            .Padding(10)
                            .Column(productsCol =>
                            {
                                int index = 1;
                                foreach (var item in model.InventoryItems)
                                {
                                    productsCol.Item().Text($"{index}. {item.ProductName}")
                                        .FontSize(11)
                                        .Bold();

                                    productsCol.Item().PaddingTop(4).Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {

                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(2);
                                        });

                                        table.Cell().PaddingVertical(2).PaddingHorizontal(4)
                                           .Text("UPC").FontSize(9).FontColor(Colors.Grey.Darken1).SemiBold();
                                        table.Cell().PaddingVertical(2).PaddingHorizontal(5)
                                            .Text("Availability").FontSize(9).FontColor(Colors.Grey.Darken1).SemiBold();
                                        table.Cell().PaddingVertical(2).PaddingHorizontal(4)
                                            .Text("Each").FontSize(9).FontColor(Colors.Grey.Darken1).SemiBold();
                                        table.Cell().PaddingVertical(2).PaddingHorizontal(4)
                                            .Text("Quantity").FontSize(9).FontColor(Colors.Grey.Darken1).SemiBold();
                                        table.Cell().PaddingVertical(2).PaddingHorizontal(4)
                                            .Text("Total").FontSize(9).FontColor(Colors.Grey.Darken1).SemiBold();

                                        table.Cell().PaddingVertical(2).PaddingHorizontal(4)
                                            .Text($" {item.ProductUpc}").FontSize(10).FontColor(Colors.Black);
                                        table.Cell().PaddingVertical(2).PaddingHorizontal(5)
                                           .Text($"In stock ({item.AvailableAmount})").FontSize(10).FontColor(Colors.Black);
                                        table.Cell().PaddingVertical(2).PaddingHorizontal(4)
                                            .Text($"${item.Price:0.00}").FontSize(10).FontColor(Colors.Black);
                                        table.Cell().PaddingVertical(2).PaddingHorizontal(4)
                                            .Text($"{item.ReservedAmount}").FontSize(10).FontColor(Colors.Black);
                                        table.Cell().PaddingVertical(2).PaddingHorizontal(4)
                                            .Text($"${(item.Price * item.ReservedAmount):0.00}").FontSize(10).FontColor(Colors.Black);
                                    });


                                    productsCol.Item().PaddingVertical(15);

                                    index++;
                                }
                            });

                        // TOTALS SECTION
                        col.Item()
                            .PaddingTop(20)
                            .BorderTop(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .PaddingTop(10)
                            .AlignRight()
                            .Table(total =>
                            {
                                total.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                void AddRow(string label, string value, bool isTotal = false)
                                {
                                    total.Cell().PaddingVertical(4).AlignLeft().Text(label)
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor(Colors.Grey.Darken1);

                                    total.Cell().PaddingVertical(4).AlignRight().Text(value)
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor(isTotal ? Colors.Black : Colors.Grey.Darken1);
                                }

                                AddRow("Subtotal", $"${model.Subtotal:0.00}");
                                AddRow("Total sale", $"-${model.TotalSale:0.00}");
                                AddRow("Total", $"${model.TotalAmount:0.00}", true);
                            });
                    });
                });
            });

            document.GeneratePdf(stream);
            stream.Position = 0;
            return await Task.FromResult(stream);
        }
    }
}