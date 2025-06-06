using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services.IImageValidatorService;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UploadInventoryItemsImages(Guid Id, IFormFile Image) : IRequest;

public class UploadInventoryItemsImagesHandler(
    IInventoryItemRepository inventoryItemRepository,
    IAzureBlobService azureBlobService,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions,
    AppDbContext dbContext,
    IImageValidatorService imageValidatorService)
    : IRequestHandler<UploadInventoryItemsImages>
{
    public async Task Handle(UploadInventoryItemsImages request, CancellationToken cancellationToken)
    {
        var spec = new SpecificationBuilder<InventoryItemEntity>()
            .AddCriteria(x => x.Id == request.Id)
            .AddInclude(x => x.Images)
            .Build();
        var item = await inventoryItemRepository.GetAsync(spec, cancellationToken) ?? throw new NotFoundException("No item with such id");

        if (request.Image == null) throw new BadRequestException("No image provided");

        await imageValidatorService.ValidateImageAsync(request.Image, cancellationToken);

        var fileName = $"Inventory-item-images/{item.Id}/{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";
        var container = azureBlobStorageOptions.Value.ContainerName;
        var category = Shared.Enums.BlobContentCategoryEnum.Image;
        using var stream = request.Image.OpenReadStream();

        string? blobUrl = null;

        try
        {
            blobUrl = await azureBlobService.UploadAsync(fileName, stream, container, category);

            var maxOrder = item.Images.Any() ? item.Images.Max(i => i.Order) : -1;
            var nextOrder = maxOrder + 1;

            var imageEntity = new InventoryItemImageEntity
            {
                InventoryItemId = item.Id,
                BlobUrl = blobUrl,
                Order = nextOrder
            };

            await dbContext.InventoryItemImages.AddAsync(imageEntity, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            if (blobUrl != null)
            {
                await azureBlobService.DeleteByUrlAsync(blobUrl);
            }

            throw new ApplicationException("Unable to upload image", ex);
        }
    }
}