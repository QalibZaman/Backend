using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Queries;

public record GetBrandInfoQuery(int BrandId, int ShopId) : IRequest<BrandResponse>;

public sealed class GetBrandInfoQueryHandler(
    IBrandRepository brandRepository,
    TypeAdapterConfig config)
    : IRequestHandler<GetBrandInfoQuery, BrandResponse>
{
    public async Task<BrandResponse> Handle(GetBrandInfoQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetBrandsByIdSpecification(request.BrandId);

        var brand = await brandRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException($"Brand with id {request.BrandId} not found.");

        if (brand.ShopId != request.ShopId)
            throw new MyUnauthorizedException("Brand does not belong to the current shop.");

        return brand.Adapt<BrandResponse>(config);
    }
}
