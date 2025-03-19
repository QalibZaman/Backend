using System.Globalization;
using CsvHelper.Configuration;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.CsvFileMap;

public sealed class InventoryItemMap : ClassMap<InventoryItemEntity>
{
    public InventoryItemMap()
    {
        Map(x => x.ProductUpc).Name("Product UPC");
        Map(x => x.ProductName).Name("Product Name");
        Map(x => x.ProductLocation).Name("Product Location");
        Map(x => x.Category.Name).Name("Product Category");
        Map(x => x.AvailableAmount).Name("Available Amount");
        Map(x => x.ReservedAmount).Name("Reserved Amount");
        Map(x => x.OnHandAmount).Name("On-hand Amount");
        Map(x => x.Price).Name("Price");
        Map(x => x.Status).Name("Status");
        Map(x => x.Description).Name("Description");
    }
}