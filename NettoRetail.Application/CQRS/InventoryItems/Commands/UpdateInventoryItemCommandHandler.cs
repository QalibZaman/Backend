using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateInventoryItemCommand(Guid Id, int ShopId, UpdateInventoryItemModel Model) : IRequest;

public class UpdateInventoryItemCommandHandler(
	IInventoryItemRepository inventoryItemRepository,
	ICategoryRepository categoryRepository,
	IBrandRepository brandRepository)
	: IRequestHandler<UpdateInventoryItemCommand>
{
	public async Task Handle(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
	{
		var itemByIdSpecification = new GetInventoryItemByIdAndShopIdSpecification(request.Id, request.ShopId);
		var inventoryItem = await inventoryItemRepository.GetAsync(itemByIdSpecification, cancellationToken)
			?? throw new NotFoundException("There is no inventory item with such id.");

		await ValidateRequestModel(request, inventoryItem, cancellationToken);

		request.Model.Adapt(inventoryItem);

		inventoryItem.UpdateStatus();

		await inventoryItemRepository.UpdateAsync(inventoryItem, cancellationToken);
	}

	private async Task ValidateRequestModel(UpdateInventoryItemCommand request, InventoryItemEntity inventoryItem, CancellationToken cancellationToken)
	{
		await ValidateUpc(request, inventoryItem, cancellationToken);
		await ValidateCategory(request, cancellationToken);
		await ValidateBrand(request, cancellationToken);
		ValidatePromotionPrice(request);
	}
	private async Task ValidateUpc(UpdateInventoryItemCommand request, InventoryItemEntity inventoryItem, CancellationToken cancellationToken)
	{
		if (request.Model.ProductUpc.HasValue && inventoryItem.ProductUpc != request.Model.ProductUpc)
		{
			var spec = new GetProductByUpcSpecification(request.Model.ProductUpc.Value);
			if (await inventoryItemRepository.Exists(spec, cancellationToken))
				throw new ConflictException("This product UPC is already registered.");
		}
	}
	private async Task ValidateCategory(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
	{
		if (request.Model.CategoryId is null) return;

		var spec = new GetCategoryByIdAndShopIdSpecification(request.Model.CategoryId.Value, request.ShopId);
		if (!await categoryRepository.Exists(spec, cancellationToken))
			throw new NotFoundException("There is no category with such id.");
	}
	private async Task ValidateBrand(UpdateInventoryItemCommand request, CancellationToken cancellationToken)
	{
		if (request.Model.BrandId is null) return;

		var spec = new GetBrandsByIdAndShopIdSpecification(request.Model.BrandId.Value, request.ShopId);
		if (!await brandRepository.Exists(spec, cancellationToken))
			throw new NotFoundException("There is no brand with such id.");
	}
	private void ValidatePromotionPrice(UpdateInventoryItemCommand request)
	{
		if (request.Model.IsPromoted == true && request.Model.PromotionPrice == null)
			throw new BadRequestException("Promotion price must be provided for promoted products.");

		if (request.Model.IsPromoted == false && request.Model.PromotionPrice != null)
			throw new BadRequestException("Promotion price cannot be provided for non-promoted products.");

		if (request.Model.PromotionPrice != null && request.Model.PromotionPrice >= request.Model.Price)
			throw new BadRequestException("Promotion price must be less than the regular price.");
	}
}