using MapsterMapper;
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
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Brands.Commands;

public record CreateBrandCommand(CreateBrandRequest Model, int ShopId) : IRequest<BrandResponse>;

public class CreateBrandCommandHandler(
    IBrandRepository brandRepository,
    IAzureBlobService azureBlobService,
    IBlobNameGenerator blobNameGenerator,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        if (request.Model.WebsiteUrl != null)
        {
            var specification = new GetBrandsByWebsiteUrlSpecification(request.Model.WebsiteUrl);

            if (await brandRepository.ExistsAsync(specification, cancellationToken))
            {
                throw new BadRequestException(HandlerConstants.Brand.WebsiteUrlUniqueMessage);
            }
        }

        var containerName = azureBlobStorageOptions.Value.ContainerName;

        var brand = mapper.Map<BrandEntity>(request.Model);

        brand.ShopId = request.ShopId;

        using var transaction = unitOfWork.BeginTransaction();

        await brandRepository.AddAsync(brand, cancellationToken);

        if (request.Model.LogoFile is { Length: > 0 } file)
        {
            brand.LogoImageUrl = await UpdateLogoImageUrlAsync(request, file, brand, containerName);

            await brandRepository.UpdateAsync(brand, cancellationToken);
        }

        transaction.Commit();
        return mapper.Map<BrandResponse>(brand);
    }

    private async Task<string> UpdateLogoImageUrlAsync(CreateBrandCommand request, IFormFile file, BrandEntity brand, string containerName)
    {
        var fileName = blobNameGenerator.GenerateBlobName(
            file.FileName, request.ShopId, false);

        await using var stream = file.OpenReadStream();
        brand.LogoImageUrl = await azureBlobService.UploadAsync(
            fileName, stream, containerName, BlobContentCategoryEnum.Image);

        return brand.LogoImageUrl;
    }
}