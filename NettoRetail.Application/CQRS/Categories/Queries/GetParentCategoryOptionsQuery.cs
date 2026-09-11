using MediatR;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record GetParentCategoryOptionsQuery(string? SearchTerm, int ShopId) : IRequest<List<ParentCategoryOptionResponse>>;

public class GetParentCategoryOptionsQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetParentCategoryOptionsQuery, List<ParentCategoryOptionResponse>>
{
    public async Task<List<ParentCategoryOptionResponse>> Handle(GetParentCategoryOptionsQuery request, CancellationToken cancellationToken)
    {
        var builder = new SpecificationBuilder<CategoryEntity>()
            .Add<GetCategoriesByShopIdSpecification>(request.ShopId)
            .AddOrderBy(c => c.Name);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            builder.Add<GetCategoriesByPartialNameSpecification>(request.SearchTerm);
        }

        var specification = builder.Build();

        var categories = await categoryRepository.GetAllBySpecAsync(specification, cancellationToken);

        var options = categories
            .Select(c => new ParentCategoryOptionResponse { Id = c.Id, Name = c.Name })
            .ToList();

        options.Insert(0, new ParentCategoryOptionResponse { Id = null, Name = "Empty" });

        return options;
    }
}