using Azure.Storage.Sas;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Queries;

public record GetBrandImageQuery(int Id, int ShopId) : IRequest<string>;

public class GetBrandImageQueryHandler(
    IBrandRepository brandRepository,
    IAzureBlobService azureBlobService) : IRequestHandler<GetBrandImageQuery, string>
{
    public async Task<string> Handle(GetBrandImageQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetBrandByIdAndShopIdSpecification(request.Id, request.ShopId);

        var brand = await brandRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException($"Brand with ID {request.Id} not found.");

        if (string.IsNullOrWhiteSpace(brand.LogoImageUrl))
            throw new NotFoundException($"Brand with ID {request.Id} does not have a logo image.");

        var sasUrl = azureBlobService.GenerateBlobSasUrlFromUrl(brand.LogoImageUrl, BlobSasPermissions.Read);
        return sasUrl;
    }
}
