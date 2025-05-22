using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Queries;

public record CreateBrandCommand(CreateBrandRequest Model, int ShopId) : IRequest<BrandResponse>;

public class CreateBrandCommandHandler(
    IBrandRepository brandRepository,
    IAzureBlobService azureBlobService,
    IOptions<ConnectionStrings> connectionStrings,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions) : IRequestHandler<CreateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var getBrandsByNameSpecification = new GetBrandsByNameSpecification(request.Model.Name);

        if (await brandRepository.Exists(getBrandsByNameSpecification, cancellationToken))
        {
            throw new BadRequestException("Brand's name must be unique");
        }

        if (request.Model.WebsiteUrl != null)
        {
            var getBrandsByWebsiteUrlSpecification = new GetBrandsByWebsiteUrlSpecification(request.Model.WebsiteUrl);

            if (await brandRepository.Exists(getBrandsByWebsiteUrlSpecification, cancellationToken))
            {
                throw new BadRequestException("Brand's website url must be unque");
            }
        }

        var azureConnectionString = connectionStrings.Value.AzureConnection;
        var containerName = azureBlobStorageOptions.Value.ContainerName;

        var brand = request.Model.Adapt<BrandEntity>();

        brand.ShopId = request.ShopId;

        string? fileName = null;

        if (request.Model.LogoFile != null)
        {
            fileName = $"{ImageCategoryEnum.BrandLogos.GetDirectory()}/{Guid.NewGuid()}{Path.GetExtension(request.Model.LogoFile.FileName)}";
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