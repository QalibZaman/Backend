using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Brands.Queries;

public record CreateBrandCommand(CreateBrandRequest Model, int ShopId) : IRequest<BrandResponse>;

// Commented azureblobservice for now, because it constantly throws an exception due to connectionString being empty
public class CreateBrandCommandHandler(
    IBrandRepository brandRepository,
    //IAzureBlobService azureBlobService,
    IUnitOfWork unitOfWork,
    IOptions<ConnectionStrings> connectionStrings,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions) : IRequestHandler<CreateBrandCommand, BrandResponse>
{
    public async Task<BrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        if (await brandRepository.Exists(new GetBrandsByNameSpecification(request.Model.Name), cancellationToken) ||
            await brandRepository.Exists(new GetBrandsByWebsiteUrlSpecification(request.Model.WebsiteUrl), cancellationToken))
        {
            throw new BadRequestException("Brand's name must be unque");
        }

        var azureConnectionString = connectionStrings.Value.AzureConnection;
        var brandLogoContainerName = azureBlobStorageOptions.Value.BrandLogoContainerName;

        var brand = request.Model.Adapt<BrandEntity>();
        brand.ShopId = request.ShopId;

        string? fileName = null;

        if (request.Model.LogoFile != null)
        {
            fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Model.LogoFile.FileName)}";
        }

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            if (fileName != null)
            {
                brand.LogoImageUrl = Path.Combine(
                    azureConnectionString,
                    brandLogoContainerName,
                    fileName);
            }

            await brandRepository.AddAsync(brand, cancellationToken);

            if (fileName != null)
            {
                //await using var stream = request.Model.LogoFile!.OpenReadStream();
                //await azureBlobService.UploadAsync(fileName, stream, brandLogoContainerName);
            }

            transaction.Commit();
            return brand.Adapt<BrandResponse>();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
