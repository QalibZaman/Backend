using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services.IImageValidatorService;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UploadInventoryItemsImages(Guid Id, IFormFile Image) : IRequest;

public class UploadInventoryItemsImagesHandler(InventoryItemRepository inventoryItemRepository,
    IAzureBlobService azureBlobService,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions,
    IImageValidatorService imageValidatorService) : IRequestHandler<UploadInventoryItemsImages>
{
    public async Task Handle(UploadInventoryItemsImages request, CancellationToken cancellationToken)
    {
        var specification = new GetInventoryItemByIdSpecification(request.Id);
        var item = await inventoryItemRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("No item with such id");

        if (request.Image == null) throw new BadRequestException("No image provided");

        await imageValidatorService.ValidateImageAsync(request.Image, cancellationToken);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
        var container = azureBlobStorageOptions.Value.ContainerName;
        var category = Shared.Enums.BlobContentCategoryEnum.Image;
        var stream = request.Image.OpenReadStream();

        var blobUrl = await azureBlobService.UploadAsync(fileName, stream, container, category);
    }
}
