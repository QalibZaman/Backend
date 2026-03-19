using System.Globalization;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Services.IPdfGenerator.Utilities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator;
public class PdfGenerator : IPdfGenerator
{
    public async Task<Stream> PrintOrderDetailsAsync(PrintOrderDetailsModel model)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var stream = new MemoryStream();

        var document = Document.Create((container) =>
        {
            container.Page((page) =>
            {
                page.ConfigureSettingsForOrderPrint();

                DrawContent(page, model);
            });
        });

        document.GeneratePdf(stream);
        stream.Position = 0;
        return await Task.FromResult(stream);
    }

    private static void DrawContent(PageDescriptor page, PrintOrderDetailsModel model)
    {
        DrawDocumentHeader(page, $"Order #{model.Id}");

        page.Content()
            .Column((column) =>
            {
                DrawOrderInfoBody(column, model);

                DrawStatusHistoryTable(column, model);

                DrawProductsAndPayment(column, model);

                DrawFooter(column, model.Subtotal, model.TotalSale, model.TotalAmount);
            });
    }

    private static void DrawDocumentHeader(PageDescriptor page, string headerText)
    {
        page.Header()
            .Component(new HeaderComponent(
                new HeaderConfiguration(
                    Text: headerText,
                    FontSizePx: 24,
                    PaddingBottomPx: 32)));
    }

    private static void DrawSectionHeader(ColumnDescriptor column, string headerText, int horizontalLineWidthPx, int paddingTopPx = 0)
    {
        column.Item()
            .Component(new SectionHeaderComponent(
                new SectionHeaderConfiguration(
                    Text: headerText,
                    HorizontalLineWidthPx: horizontalLineWidthPx,
                    PaddingTopPx: paddingTopPx)));
    }

    private static void DrawOrderInfoBody(ColumnDescriptor column, PrintOrderDetailsModel model)
    {
        DrawSectionHeader(column, "Order info", 128);

        column.Item()
            .Component(new OrderInfoComponent(model));
    }

    private static void DrawStatusHistoryTable(ColumnDescriptor column, PrintOrderDetailsModel model)
    {
        column.Item()
            .Component(new StatusHistoryComponent(model));
    }

    private static void DrawProductsAndPayment(ColumnDescriptor column, PrintOrderDetailsModel model)
    {
        DrawSectionHeader(column, "Products and payment", 164, 24);

        column.Item()
            .Component(new TotalAmountComponent(model.TotalProducts, model.TotalQuantity));

        column.Item()
            .Component(new OrderedItemListComponent(model));
    }

    private static void DrawFooter(ColumnDescriptor column, decimal subtotal, decimal totalSale, decimal totalAmount)
    {
        column.Item()
            .PaddingTop(Converters.ConvertPixelsToPoints(32))
            .PaddingBottom(Converters.ConvertPixelsToPoints(24))
            .Height(Converters.ConvertPixelsToPoints(2))
            .Background(UserColors.FooterDividerColor);

        (string Title, string Value)[] titleValuePairs = [
            ("Subtotal", subtotal.ToString("C", CultureInfo.GetCultureInfo("en-US"))),
            ("Total sale", $"-{totalSale.ToString("C", CultureInfo.GetCultureInfo("en-US"))}"),
            ("Total", totalAmount.ToString("C", CultureInfo.GetCultureInfo("en-US")))
        ];

        foreach (var pair in titleValuePairs)
        {
            column.Item()
                .Component(new FooterComponent(pair.Title, pair.Value));
        }
    }
}