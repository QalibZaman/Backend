using CsvHelper.Configuration.Attributes;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.CsvFileModel;

public class InventoryItemCsvModel
{
    [Name("Product UPC")]
    public required long ProductUpc { get; set; }

    [Name("Product Name")]
    public required string ProductName { get; set; }

    [Name("Product Location")]
    public required string ProductLocation { get; set; }

    [Name("Brand")]
    public required string Brand { get; set; }

    [Name("Product Category")]
    public required string ProductCategory { get; set; }

    [Name("Available Amount")]
    public required int AvailableAmount { get; set; }

    [Name("Reserved Amount")]
    public required int ReservedAmount { get; set; }

    [Name("On-hand Amount")]
    public required int OnHandAmount { get; set; }

    [Name("Price")]
    public required decimal Price { get; set; }

    [Name("Status")]
    public required InventoryItemStatusEnum Status { get; set; }

    [Name("Description")]
    public required string Description { get; set; }

    [Ignore]
    public int? CategoryId { get; set; }

    [Ignore]
    public int? ShopId { get; set; }
}