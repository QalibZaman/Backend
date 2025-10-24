using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Application.Services.IBlobNameGenerator;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Brands.Commands;

public record UpdateBrandCommand(int Id, string Name, string? Description, IFormFile? Logo, string? WebsiteUrl, int ShopId) : IRequest<BrandResponse>;

public class UpdateBrandCommandHandler(IBrandRepository brandRepository,
    IAzureBlobService azureBlobService,
    IBlobNameGenerator blobNameGenerator,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions) : IRequestHandler<UpdateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<BrandEntity>();
        specificationBuilder.Add<GetBrandsByIdSpecification>(request.Id)
                            .Add<GetBrandsByShopIdSpecification>(request.ShopId);

        var brand = await brandRepository.GetAsync(specificationBuilder.Build(), cancellationToken)
                   ?? throw new NotFoundException("Brand not found.");

        // Upload new logo
        if (request.Logo != null)
        {
            var blobPath = blobNameGenerator.GenerateBlobName(request.Logo.FileName, ImageCategoriesEnum.Brands, request.ShopId, false);

            var logoUrl = await azureBlobService.UploadAsync(
                blobPath,
                request.Logo.OpenReadStream(),
                azureBlobStorageOptions.Value.ContainerName,
                BlobContentCategoryEnum.Image,
                true);

            brand.LogoImageUrl = logoUrl;
        }

        request.Adapt(brand);
        await brandRepository.UpdateAsync(brand, cancellationToken);

        return brand.Adapt<BrandResponse>();
    }
}
