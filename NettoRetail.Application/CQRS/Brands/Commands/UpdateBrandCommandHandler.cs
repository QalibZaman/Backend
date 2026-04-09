using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Application.Services.IBlobNameGenerator;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Brands.Commands;

public record UpdateBrandCommand(int Id, UpdateBrandRequest Model, int ShopId) : IRequest<BrandResponse>;

public class UpdateBrandCommandHandler(IBrandRepository brandRepository,
    IAzureBlobService azureBlobService,
    IBlobNameGenerator blobNameGenerator,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions,
    IMapper mapper) : IRequestHandler<UpdateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<BrandEntity>();
        specificationBuilder.Add<GetBrandsByIdSpecification>(request.Id)
                            .Add<GetBrandsByShopIdSpecification>(request.ShopId);
        var spec = specificationBuilder.Build();

        var brand = await brandRepository.GetAsync(spec, cancellationToken)
                   ?? throw new NotFoundException("Brand not found.");

        // Upload new logo
        if (request.Model.Logo != null)
        {
            var blobPath = blobNameGenerator.GenerateBlobName(request.Model.Logo.FileName, ImageCategoriesEnum.Brands, request.ShopId, false);

            var logoUrl = await azureBlobService.UploadAsync(
                blobPath,
                request.Model.Logo.OpenReadStream(),
                azureBlobStorageOptions.Value.ContainerName,
                BlobContentCategoryEnum.Image,
                true);

            brand.LogoImageUrl = logoUrl;
        }

        mapper.Map(request.Model, brand);
        await brandRepository.UpdateAsync(brand, cancellationToken);

        return mapper.Map<BrandResponse>(brand);
    }
}
