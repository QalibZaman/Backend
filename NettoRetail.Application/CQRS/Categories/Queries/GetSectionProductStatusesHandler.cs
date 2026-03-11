using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Categories.Queries;

public record GetCategoryProductStatusesCommand(int ShopId, int CategoryId) : IRequest<GetCategoryProductStatusesResponse>;
public class GetCategoryProductStatusesCammandHandler(ICategoryRepository categoryRepository, IMapper mapper)
    : IRequestHandler<GetCategoryProductStatusesCommand, GetCategoryProductStatusesResponse>
{
    public async Task<GetCategoryProductStatusesResponse> Handle(GetCategoryProductStatusesCommand request, CancellationToken cancellationToken)
    {
        if (request.CategoryId <= 0) throw new BadRequestException("Section ID cant be 0 or negative number");

        var spec = new GetCategoryByIdAndShopIdSpecification(request.CategoryId, request.ShopId)
            .AddInclude(x => x.InventoryItems);

        var category = await categoryRepository.GetAsync(spec, cancellationToken) ??
            throw new NotFoundException($"Category {request.CategoryId} not found in shop {request.ShopId}");

        var response = mapper.Map<GetCategoryProductStatusesResponse>(category);

        return response;
    }
}
