using Azure.Storage.Sas;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.ProductBanners.Queries
{
    public record GetInventoryItemBannersQuery(Guid InventoryItemId) : IRequest<List<GetInventoryItemBannerResponse>>;

    public class GetInventoryItemBannersQueryHandler : IRequestHandler<GetInventoryItemBannersQuery, List<GetInventoryItemBannerResponse>>
    {
        private readonly IInventoryItemBannerRepository _bannerRepository;
        private readonly IAzureBlobService _blobService;
        private readonly string _containerName = "dot-net";

        public GetInventoryItemBannersQueryHandler(
            IInventoryItemBannerRepository bannerRepository,
            IAzureBlobService blobService)
        {
            _bannerRepository = bannerRepository;
            _blobService = blobService;
        }

        public async Task<List<GetInventoryItemBannerResponse>> Handle(GetInventoryItemBannersQuery request, CancellationToken cancellationToken)
        {
            var banners = await _bannerRepository.GetByInventoryItemIdAsync(
                request.InventoryItemId,
                new Specification<InventoryItemBannerEntity>(b => true),
                cancellationToken);

            var responses = new List<GetInventoryItemBannerResponse>();

            foreach (var banner in banners.OrderBy(b => b.OrderIndex))
            {
                string blobPath = $"images/{banner.FileIdentifier}";
                var blobUrl = _blobService.GetBlobUrl(_containerName, blobPath);

                var sasToken = _blobService.GenerateBlobSasToken(
                    _containerName,
                    blobPath,
                    BlobSasPermissions.Read);

                responses.Add(new GetInventoryItemBannerResponse
                {
                    BannerId = banner.Id,
                    OrderIndex = banner.OrderIndex,
                    FileName = banner.FileName,
                    FileUrl = $"{blobUrl}?{sasToken}"
                });
            }

            return responses;
        }
    }
}
