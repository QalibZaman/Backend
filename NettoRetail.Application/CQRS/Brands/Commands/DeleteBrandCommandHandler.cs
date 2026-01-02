using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Commands;

public record DeleteBrandCommand(int Id, int ShopId) : IRequest;

public class DeleteBrandCommandHandler(IBrandRepository brandRepository,
    IAzureBlobService azureBlobService,
    IInventoryItemRepository inventoryItemRepository) : IRequestHandler<DeleteBrandCommand>
{
    public async Task Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var brandSpecification = new GetBrandsByIdAndShopIdSpecification(request.Id, request.ShopId);

        var brand = await brandRepository.GetAsync(brandSpecification, cancellationToken)
            ?? throw new NotFoundException($"Your shop has no brands with id: {request.Id}");

        var itemsSpecification = new GetInventoryItemByBrandIdSpecification(request.Id);

        var associatedItemsExists = await inventoryItemRepository.Exists(itemsSpecification, cancellationToken);
        if (associatedItemsExists)
            throw new ConflictException("Cannot delete brands when associated items exist");

        if (brand.LogoImageUrl != null)
            await azureBlobService.DeleteByUrlAsync(brand.LogoImageUrl);

        await brandRepository.DeleteAsync(brand, cancellationToken);
    }
}
