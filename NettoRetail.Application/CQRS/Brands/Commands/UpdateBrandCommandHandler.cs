using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Brands.Commands;

public record UpdateBrandCommand(int Id, string Name, string? Description, IFormFile? Logo, string? WebsiteUrl, int ShopId) : IRequest;

public class UpdateBrandCommandHandler(IBrandRepository brandRepository, IAzureBlobService azureBlobService) : IRequestHandler<UpdateBrandCommand>
{
    public async Task Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<BrandEntity>();
        specificationBuilder.Add<GetBrandsByIdSpecification>(request.Id)
                            .Add<GetBrandsByShopIdSpecification>(request.ShopId);

        var brand = await brandRepository.GetAsync(specificationBuilder.Build(), cancellationToken) ?? throw new NotFoundException("Brand not found.");

        request.Adapt(brand);

        await brandRepository.UpdateAsync(brand, cancellationToken);
   }
}
