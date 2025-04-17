using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Commands;

public record UpdateBrandCommand(int Id, string Name, string Description, IFormFile? Logo, string WebsiteUrl, int ShopId) : IRequest<BrandResponse>;

public class UpdateBrandCommandHandler(IBrandRepository brandRepository) : IRequestHandler<UpdateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetBrandsByIdShopIdSpecification();
        var brand = await brandRepository.GetAsync(specification.ByIdAndShopId(request.Id, request.ShopId), cancellationToken) ?? throw new NotFoundException("Brand not found.");

        request.Adapt(brand);

        await brandRepository.UpdateAsync(brand, cancellationToken);

        var result = brand.Adapt<BrandResponse>();
        return result;
   }
}
