using Azure.Storage.Sas;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.ProductBanners.Commands
{
    public record AddInventoryItemBannersCommand(
        Guid InventoryItemId,
        IFormFile[] Files) : IRequest<List<GetInventoryItemBannerResponse>>;

    public class AddInventoryItemBannerCommandHandler(
        IInventoryItemBannerRepository bannerRepository,
        IInventoryItemRepository inventoryItemRepository,
        IAzureBlobService blobService)
        : IRequestHandler<AddInventoryItemBannersCommand, List<GetInventoryItemBannerResponse>>
    {
        private const int MaxImagesPerProduct = 8;

        public async Task<List<GetInventoryItemBannerResponse>> Handle(AddInventoryItemBannersCommand request, CancellationToken cancellationToken)
        {
            if (request.Files == null || !request.Files.Any())
                return new List<GetInventoryItemBannerResponse>();

            var itemSpec = new GetItemByIdSpecification(request.InventoryItemId);
            if (!await inventoryItemRepository.Exists(itemSpec, cancellationToken))
            {
                throw new NotFoundException("The product does not exist.");
            }

            var existingImages = await bannerRepository.GetByInventoryItemIdAsync(
                request.InventoryItemId,
                new Specification<InventoryItemBannerEntity>(b => true),
                cancellationToken) ?? [];
            if (existingImages.Count + request.Files.Length > MaxImagesPerProduct)
            {
                throw new BadRequestException($"Limit to {MaxImagesPerProduct} images per product.");
            }

            var responses = new List<GetInventoryItemBannerResponse>();
            const string containerName = "dot-net";
            var nextOrderIndex = await bannerRepository
                .GetMaxOrderIndexAsync(request.InventoryItemId, cancellationToken) + 1;

            foreach (var file in request.Files)
            {
                var uniqueId = Guid.NewGuid().ToString();

                using var stream = file.OpenReadStream();
                await blobService.UploadAsync(
                    uniqueId,
                    stream,
                    containerName,
                    BlobContentCategoryEnum.Image,
                    contentType: file.ContentType);

                var banner = new InventoryItemBannerEntity
                {
                    InventoryItemId = request.InventoryItemId,
                    FileIdentifier = uniqueId,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    OrderIndex = nextOrderIndex++
                };

                var addedBanner = await bannerRepository.AddAsync(banner, cancellationToken);

                string blobPath = $"images/{addedBanner.FileIdentifier}";
                var blobUrl = blobService.GetBlobUrl(containerName, blobPath);
                var sasToken = blobService.GenerateBlobSasToken(
                    containerName,
                    blobPath,
                    BlobSasPermissions.Read);

                responses.Add(new GetInventoryItemBannerResponse
                {
                    BannerId = addedBanner.Id,
                    OrderIndex = addedBanner.OrderIndex,
                    FileName = addedBanner.FileName,
                    FileUrl = $"{blobUrl}?{sasToken}"
                });
            }

            return responses;
        }
    }
}
