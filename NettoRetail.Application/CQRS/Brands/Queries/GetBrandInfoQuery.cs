using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Queries;

public record GetBrandInfoQuery(int BrandId, int ShopId) : IRequest<BrandResponse>;

public class GetBrandInfoQueryHandler(IBrandRepository brandRepository) : IRequestHandler<GetBrandInfoQuery, BrandResponse>
{
    public async Task<BrandResponse> Handle(GetBrandInfoQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetBrandsByIdSpecification(request.BrandId);
        var brand = await brandRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException($"No brand with id: {request.BrandId}");

        if (brand.ShopId != request.ShopId)
        {
            throw new MyUnauthorizedException("This item is inaccessible");
        }

        var brandResponse = brand.Adapt<BrandResponse>();
        return brandResponse;
    }
}
