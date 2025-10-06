using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.Comparison;
using NettoRetail.DataAccess.Specifications.Comparison;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Comparison.Commands;

public record AddItemToComparisonCommand(string UserId, Guid InventoryItemId) : IRequest<ComparisonItemResponse>;

public class AddItemToComparisonHandler(
    IComparisonRepository comparisonRepository,
    IInventoryItemRepository inventoryItemRepository)
    : IRequestHandler<AddItemToComparisonCommand, ComparisonItemResponse>
{
    public async Task<ComparisonItemResponse> Handle(AddItemToComparisonCommand request, CancellationToken cancellationToken)
    {
        // 1. Get inventory item
        var itemSpec = new SpecificationBuilder<InventoryItemEntity>()
            .AddCriteria(x => x.Id == request.InventoryItemId)
            .Build();

        var inventoryItem = await inventoryItemRepository.GetAsync(itemSpec, cancellationToken)
            ?? throw new NotFoundException("Inventory item not found");

        // 2. Get or create comparison list for user/category
        var listSpec = new SpecificationBuilder<ComparisonListEntity>()
            .AddCriteria(x => x.UserId == request.UserId && x.CategoryId == inventoryItem.CategoryId)
            .AddInclude(x => x.ComparisonListItems)
            .Build();

        var comparisonList = await comparisonRepository.GetAsync(listSpec, cancellationToken);

        if (comparisonList == null)
        {
            comparisonList = new ComparisonListEntity
            {
                UserId = request.UserId,
                CategoryId = inventoryItem.CategoryId,
                CreatedAtUtc = DateTime.UtcNow,
                ComparisonListItems = new List<ComparisonListItemEntity>()
            };
            await comparisonRepository.AddAsync(comparisonList, cancellationToken);
        }

        // 3. Business rules
        if (comparisonList.ComparisonListItems.Any(x => x.InventoryItemId == request.InventoryItemId))
            throw new BadRequestException("Item already exists in comparison list.");

        if (comparisonList.ComparisonListItems.Count >= 4)
            throw new BadRequestException("You can compare up to 4 products only.");

        if (comparisonList.ComparisonListItems.Count > 0 &&
            comparisonList.CategoryId != inventoryItem.CategoryId)
            throw new BadRequestException("You are trying to compare items from different category.");

        // 4. Add new item
        var comparisonItem = new ComparisonListItemEntity
        {
            ComparisonListId = comparisonList.Id,
            ComparisonList = comparisonList,
            InventoryItemId = inventoryItem.Id,
            InventoryItem = inventoryItem,
            ProductName = inventoryItem.ProductName,
            Price = inventoryItem.Price,
            PromotionPrice = inventoryItem.PromotionPrice,
            Brand = inventoryItem.Brand?.Name,
            IsPromoted = inventoryItem.IsPromoted,
            AvailableAmount = inventoryItem.AvailableAmount,
            ReservedAmount = inventoryItem.ReservedAmount,
            OnHandAmount = inventoryItem.OnHandAmount,
            ProductLocation = inventoryItem.ProductLocation,
            CreatedAtUtc = DateTime.UtcNow
        };

        comparisonList.ComparisonListItems.Add(comparisonItem);
        await comparisonRepository.UpdateAsync(comparisonList, cancellationToken);

        // 5. Map to response using Mapster
        return comparisonItem.Adapt<ComparisonItemResponse>();
    }
}
