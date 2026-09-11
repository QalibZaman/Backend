using MediatR;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record CheckCategoryNameQuery(string Name, int ShopId) : IRequest<CheckCategoryNameResponse>;

public class CheckCategoryNameQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<CheckCategoryNameQuery, CheckCategoryNameResponse>
{
    public async Task<CheckCategoryNameResponse> Handle(CheckCategoryNameQuery request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        var nameExists = await categoryRepository.ExistsAsync(
            new GetCategoryByNameSpecification(normalizedName, request.ShopId),
            cancellationToken);

        return nameExists
            ? new CheckCategoryNameResponse { IsAvailable = false, Message = "Category with this name already exists" }
            : new CheckCategoryNameResponse { IsAvailable = true, Message = "Category name is available" };
    }
}