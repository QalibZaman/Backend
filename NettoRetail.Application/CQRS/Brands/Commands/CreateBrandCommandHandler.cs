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

        var containerName = azureBlobStorageOptions.Value.ContainerName;

        var brand = request.Model.Adapt<BrandEntity>();

        brand.ShopId = request.ShopId;

        string? fileName = null;

        if (request.Model.LogoFile != null)
        {
            fileName = blobNameGenerator.GenerateBlobName(request.Model.LogoFile.FileName, ImageCategoriesEnum.Brands, request.ShopId, false);
        }

        if (fileName != null)
        {
            await using var stream = request.Model.LogoFile!.OpenReadStream();
            brand.LogoImageUrl = await azureBlobService.UploadAsync(fileName, stream, containerName, BlobContentCategoryEnum.Image);
        }

        await brandRepository.AddAsync(brand, cancellationToken);

        return brand.Adapt<BrandResponse>();
    }
}