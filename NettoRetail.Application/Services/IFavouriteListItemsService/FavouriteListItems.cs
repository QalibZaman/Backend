using NettoRetail.Application.CQRS.FavouriteListItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Services.ICartService;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IFavouriteListItemsService;

public class FavouriteListItems(
    IFavouriteListItemRepository favouriteListRepo,
    CartService cartService) : IFavouriteListItems
{
    private readonly IFavouriteListItemRepository _favouriteListRepo = favouriteListRepo;
    private readonly CartService _cartService = cartService;

    public async Task<List<Guid>> ResolveTargetIdsAsync(AddToCartFromFavouritesCommand request, object? favouriteItemsSpecification, CancellationToken cancellationToken = default)
    {
        if (request.isAll)
        {
            if (favouriteItemsSpecification is not Specification<FavouriteListItemEntity> spec)
                throw new BadRequestException("Favourite items specification is required when all=true.");

            var ids = (await _favouriteListRepo.GetManyBySpecAsync(spec, cancellationToken))
                .Select(x => x.InventoryItemId)
                .Distinct()
                .ToList();

            return ids;
        }

        if (request.InventoryItemId is Guid id && id != Guid.Empty)
            return new List<Guid> { id };

        throw new BadRequestException("inventoryItemId is required when all=false.");
    }

    public async Task<FavouriteListBulkAddResult> AddManyToCartAsync(Guid accountId, IEnumerable<Guid> inventoryItemIds, int maxDegreeOfParallelism,CancellationToken cancellationToken = default)
    {
        var ids = inventoryItemIds.Where(id => id != Guid.Empty).ToList();
        if (ids.Count == 0) return new FavouriteListBulkAddResult(0, null, null);

        var degreeOfParallelism = Math.Max(1, maxDegreeOfParallelism);
        using var gate = new SemaphoreSlim(degreeOfParallelism, degreeOfParallelism);

        int added = 0;
        CartEntity? lastCart = null;
        CartItemEntity? lastLine = null;
        var locker = new object();
        var tasks = new List<Task>(ids.Count);

        foreach (var itemId in ids)
        {
            await gate.WaitAsync(cancellationToken);
            tasks.Add(Task.Run(
                async () =>
                {
                    try
                    {
                        var cart = await _cartService.AddItemForUserAsync(accountId, itemId, cancellationToken);
                        Interlocked.Increment(ref added);
                        lock (locker)
                        {
                            lastCart = cart;
                            lastLine = cart.Items.FirstOrDefault(i => i.InventoryItemId == itemId) ?? lastLine;
                        }
                    }
                    catch
                    {
                        // Bulk mode: skip the failing item and continue
                    }
                    finally
                    {
                        gate.Release();
                    }
                },
                cancellationToken));
        }

        await Task.WhenAll(tasks);
        return new FavouriteListBulkAddResult(added, lastCart, lastLine);
    }
}
