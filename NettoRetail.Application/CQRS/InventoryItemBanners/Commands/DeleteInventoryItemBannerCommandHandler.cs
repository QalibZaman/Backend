using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.ProductBanners.Commands
{
    public record DeleteInventoryItemBannerCommand(Guid Id) : IRequest<Unit>;

    public class DeleteInventoryItemBannerCommandHandler : IRequestHandler<DeleteInventoryItemBannerCommand, Unit>
    {
        private readonly IInventoryItemBannerRepository _bannerRepository;
        private readonly IAzureBlobService _blobService;

        public DeleteInventoryItemBannerCommandHandler(
            IInventoryItemBannerRepository bannerRepository,
            IAzureBlobService blobService)
        {
            _bannerRepository = bannerRepository;
            _blobService = blobService;
        }

        public async Task<Unit> Handle(DeleteInventoryItemBannerCommand request, CancellationToken cancellationToken)
        {
            var existingBanner = await _bannerRepository.GetByIdAsync(request.Id, cancellationToken);

            if (existingBanner == null)
                throw new NotFoundException($"ProductBanner with Id {request.Id} not found.");

            if (!string.IsNullOrWhiteSpace(existingBanner.FileIdentifier))
            {
                var blobPath = $"images/{existingBanner.FileIdentifier}";
                await _blobService.DeleteBlobAsync(blobPath);
            }

            await _bannerRepository.DeleteAsync(existingBanner, cancellationToken);

            var bannersToReorder = await _bannerRepository
                .GetByInventoryItemIdAsync(
                    existingBanner.InventoryItemId,
                    new Specification<InventoryItemBannerEntity>(b => b.OrderIndex > existingBanner.OrderIndex),
                    cancellationToken);

            foreach (var banner in bannersToReorder)
            {
                banner.OrderIndex -= 1;
            }

            await _bannerRepository.UpdateManyAsync(bannersToReorder, cancellationToken);

            return Unit.Value;
        }
    }
}
