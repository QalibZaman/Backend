using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Application.Services.IBlobNameGenerator;
using NettoRetail.Application.Services.IBrandService;
using NettoRetail.DataAccess.Options;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Commands;

public record CreateBrandCommand(CreateBrandRequest Model, int ShopId) : IRequest<BrandResponse>;

public class CreateBrandCommandHandler(
    IBrandService brandService,
    IBrandRepository brandRepository,
    IAzureBlobService azureBlobService,
    IBlobNameGenerator blobNameGenerator,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions) : IRequestHandler<CreateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        if (request.ShopId <= 0) throw new ForbiddenException("Brand creation failed. No shop is linked to this account.");

        await brandService.EnsureBrandCanBeCreatedAsync(request.ShopId, request.Model.Name, request.Model.WebsiteUrl, cancellationToken);
        if (request.Model.WebsiteUrl != null)
        {
            var getBrandsByWebsiteUrlSpecification = new GetBrandsByWebsiteUrlSpecification(request.Model.WebsiteUrl);

            if (await brandRepository.Exists(getBrandsByWebsiteUrlSpecification, cancellationToken))
            {
                throw new BadRequestException("Brand's website url must be unque");
            }
        }

        var containerName = azureBlobStorageOptions.Value.ContainerName;

        var brand = request.Model.Adapt<BrandEntity>();

        brand.ShopId = request.ShopId;

        await brandRepository.AddAsync(brand, cancellationToken);

        if (request.Model.LogoFile is { Length: > 0 } file)
        {
            var fileName = blobNameGenerator.GenerateBlobName(
                file.FileName, ImageCategoriesEnum.Brands, request.ShopId, false);

            await using var stream = file.OpenReadStream();
            brand.LogoImageUrl = await azureBlobService.UploadAsync(
                fileName, stream, containerName, BlobContentCategoryEnum.Image);

            await brandRepository.UpdateAsync(brand, cancellationToken);
        }

        return brand.Adapt<BrandResponse>();
    }
}