using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Shop;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Shops.Commands;

public record ChangeLogoCommand(AddLogoRequest Model, int ShopId) : IRequest;

public class ChangeLogoCommandHandler(
    IImageValidatorService imageValidatorService,
    IAzureBlobService azureBlobService,
    IShopRepository shopRepository) : IRequestHandler<ChangeLogoCommand>
{
    private const string ContainerName = "dot-net";

    public async Task Handle(ChangeLogoCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetShopByIdSpecification(request.ShopId);
        var shop = await shopRepository.GetAsync(specification, cancellationToken);
        if (shop == null) throw new NotFoundException("Shop not found");
        if (shop.LogoUrl == null) throw new NotFoundException("Logo not found");

        var oldLogoUrl = shop.LogoUrl;
        await imageValidatorService.ValidateLogoAsync(request.Model.Logo, cancellationToken);
        shop.LogoUrl = await AddLogo(request.Model.Logo);
        await shopRepository.UpdateAsync(shop, cancellationToken);
        await azureBlobService.DeleteByUrlAsync(
            oldLogoUrl,
            cancellationToken);
    }

    private async Task<string> AddLogo(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        await using var stream = file.OpenReadStream();

        return await azureBlobService.UploadAsync(
            fileName,
            stream,
            ContainerName,
            BlobContentCategoryEnum.Image,
            overwrite: true,
            contentType: file.ContentType);
    }
}