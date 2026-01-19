using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.VariationService.VariationDetailsService
{
    public class VariationDetailsService : IVariationDetailsService
    {
        public Task InheritDetailsAsync(
            InventoryItemEntity variation,
            InventoryItemEntity parent,
            CancellationToken ct)
        {
            variation.ProductDetails.Clear();

            foreach (var parentDetail in parent.ProductDetails.OrderBy(x => x.OrderIndex))
            {
                variation.ProductDetails.Add(new ProductDetailEntity
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = variation.Id,
                    Name = parentDetail.Name,
                    TypeEnum = parentDetail.TypeEnum,
                    Unit = parentDetail.Unit,
                    Value = parentDetail.Value,
                    OrderIndex = parentDetail.OrderIndex
                });
            }

            return Task.CompletedTask;
        }

        public Task ApplyCustomDetailsAsync(
            InventoryItemEntity variation,
            List<CustomDetailModel> customDetails,
            CancellationToken ct)
        {
            // remove old variant details first
            variation.ProductDetails.Clear();

            foreach (var detail in customDetails.OrderBy(x => x.OrderIndex))
            {
                variation.ProductDetails.Add(new ProductDetailEntity
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = variation.Id,
                    Name = detail.Name,
                    TypeEnum = detail.TypeEnum,
                    Unit = detail.Unit,
                    Value = ConvertToString(detail.Value),
                    OrderIndex = detail.OrderIndex
                });
            }

            return Task.CompletedTask;
        }

        private string ConvertToString(object value)
        {
            return value switch
            {
                string s => s,
                bool b => b.ToString().ToLower(),
                int i => i.ToString(),
                decimal d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
                double dbl => dbl.ToString(System.Globalization.CultureInfo.InvariantCulture),
                _ => value.ToString() ?? ""
            };
        }
    }

}
