using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Response.Brand;
using NettoRetail.Application.Services.IBlobNameGenerator;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

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
        var specification = new GetBrandByIdAndShopIdSpecification(request.Id, request.ShopId);

        var brand = await brandRepository.GetAsync(specification, cancellationToken)
                   ?? throw new NotFoundException(HandlerConstants.Brand.BrandNotFoundMessage);

        mapper.Map(request.Model, brand);

        if (request.Model.LogoFile is { Length: > 0 })
        {
            var blobPath = blobNameGenerator.GenerateBlobName(request.Model.LogoFile.FileName, request.ShopId, false);

            var logoUrl = await azureBlobService.UploadAsync(
                blobPath,
                request.Model.LogoFile.OpenReadStream(),
                azureBlobStorageOptions.Value.ContainerName,
                BlobContentCategoryEnum.Image,
                true);

            brand.LogoImageUrl = logoUrl;
        }

        await brandRepository.UpdateAsync(brand, cancellationToken);

        return mapper.Map<BrandResponse>(brand);
    }
}
