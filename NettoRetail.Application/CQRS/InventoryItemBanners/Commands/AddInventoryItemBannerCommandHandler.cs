using Azure.Storage.Sas;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.ProductBanners.Commands
{
    public record AddInventoryItemBannersCommand(
    Guid InventoryItemId,
    IFormFile[] Files) : IRequest<List<GetInventoryItemBannerResponse>>;

    public class AddProductBannersCommandHandler : IRequestHandler<AddInventoryItemBannersCommand, List<GetInventoryItemBannerResponse>>
    {
        private readonly IInventoryItemBannerRepository _bannerRepository;
        private readonly IAzureBlobService _blobService;

        public AddProductBannersCommandHandler(
            IInventoryItemBannerRepository bannerRepository,
            IAzureBlobService blobService)
        {
            _bannerRepository = bannerRepository;
            _blobService = blobService;
        }

        public async Task<List<GetInventoryItemBannerResponse>> Handle(AddInventoryItemBannersCommand request, CancellationToken cancellationToken)
        {
            if (request.Files == null || !request.Files.Any())
                return new List<GetInventoryItemBannerResponse>();

            var responses = new List<GetInventoryItemBannerResponse>();
            string containerName = "dot-net";

            foreach (var file in request.Files)
            {
                var uniqueId = Guid.NewGuid().ToString();

                using var stream = file.OpenReadStream();
                await _blobService.UploadAsync(
                    uniqueId,
                    stream,
                    containerName,
                    BlobContentCategoryEnum.Image,
                    contentType: file.ContentType);

                var nextOrderIndex = await _bannerRepository
                    .GetMaxOrderIndexAsync(request.InventoryItemId, cancellationToken) + 1;

                var banner = new InventoryItemBannerEntity
                {
                    InventoryItemId = request.InventoryItemId,
                    FileIdentifier = uniqueId,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    OrderIndex = nextOrderIndex
                };

                var addedBanner = await _bannerRepository.AddAsync(banner, cancellationToken);

                string blobPath = $"images/{addedBanner.FileIdentifier}";
                var blobUrl = _blobService.GetBlobUrl(containerName, blobPath);
                var sasToken = _blobService.GenerateBlobSasToken(
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
