using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Utilities;

public static class BrandSpecificationFactory
{
    public static Specification<BrandEntity> BuildShopBrandsSpecification(
        int shopId,
        PaginationRequest pagination,
        string? searchTerm = null)
    {
        var builder = new SpecificationBuilder<BrandEntity>()
            .Add<GetBrandsByShopIdSpecification>(shopId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            builder.Add<GetBrandsByPartialNameSpecification>(searchTerm.Trim());
        }

        var skip = PaginationHelper.CalculateSkip(pagination.PageNumber, pagination.PageSize);

        builder.AddOrderBy(b => b.Name)
            .AddOrderBy(b => b.Id)
            .Skip(skip)
            .Take(pagination.PageSize);

        return builder.Build();
    }
}
