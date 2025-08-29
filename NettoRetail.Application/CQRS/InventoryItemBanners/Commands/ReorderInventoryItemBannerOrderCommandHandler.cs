using Azure.Storage.Sas;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.ProductBanners.Commands
{
    public record ReorderInventoryItemBannerOrderCommand(
        Guid BannerId,
        int NewOrderIndex) : IRequest<List<GetInventoryItemBannerResponse>>;


    public class ReorderInventoryItemBannerOrderCommandHandler : IRequestHandler<ReorderInventoryItemBannerOrderCommand, List<GetInventoryItemBannerResponse>>
    {
        private readonly IInventoryItemBannerRepository _bannerRepository;
        private readonly IAzureBlobService _blobService;
        private readonly string _containerName = "dot-net";

        public ReorderInventoryItemBannerOrderCommandHandler(
            IInventoryItemBannerRepository bannerRepository,
            IAzureBlobService blobService)
        {
            _bannerRepository = bannerRepository;
            _blobService = blobService;
        }

        public async Task<List<GetInventoryItemBannerResponse>> Handle(ReorderInventoryItemBannerOrderCommand request, CancellationToken cancellationToken)
        {
            var banner = await _bannerRepository.GetByIdAsync(request.BannerId, cancellationToken);
            if (banner == null)
                throw new NotFoundException($"ProductBanner with Id {request.BannerId} not found.");

            var banners = (await _bannerRepository.GetByInventoryItemIdAsync(
                banner.InventoryItemId,
                new Specification<InventoryItemBannerEntity>(null),
                cancellationToken))
                .OrderBy(b => b.OrderIndex)
                .ToList();

            if (request.NewOrderIndex < 0 || request.NewOrderIndex >= banners.Count)
                throw new BadRequestException("New order index is out of range.");

            banners.Remove(banner);
            banners.Insert(request.NewOrderIndex, banner);

            for (int i = 0; i < banners.Count; i++)
            {
                banners[i].OrderIndex = i;
                await _bannerRepository.UpdateAsync(banners[i], cancellationToken);
            }

            var responses = new List<GetInventoryItemBannerResponse>();
            foreach (var b in banners)
            {
                string blobPath = $"images/{b.FileIdentifier}";
                var blobUrl = _blobService.GetBlobUrl(_containerName, blobPath);
                var sasToken = _blobService.GenerateBlobSasToken(_containerName, blobPath, BlobSasPermissions.Read);

                responses.Add(new GetInventoryItemBannerResponse
                {
                    BannerId = b.Id,
                    OrderIndex = b.OrderIndex,
                    FileName = b.FileName,
                    FileUrl = $"{blobUrl}?{sasToken}"
                });
            }

            return responses;
        }
    }

}
