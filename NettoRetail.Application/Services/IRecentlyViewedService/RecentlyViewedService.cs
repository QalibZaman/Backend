using MapsterMapper;
using NettoRetail.Application.Models.Request.RecentlyViewedProduct;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IRecentlyViewedService;
public class RecentlyViewedService : IRecentlyViewedService
{
    private readonly IRecentlyViewedRepository _repository;
    private readonly IMapper _mapper;

    public RecentlyViewedService(IRecentlyViewedRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task AddOrUpdateAsync(Guid accountId, Guid inventoryItemId, CancellationToken cancellationToken)
    {
        var existing = await FindExistingAsync(accountId, inventoryItemId, cancellationToken);

        if (existing != null)
        {
            await UpdateTimestampAsync(existing, cancellationToken);
        }
        else
        {
            await InsertNewAsync(accountId, inventoryItemId, cancellationToken);
        }

        await EnforceMaxLimitAsync(accountId, cancellationToken);
    }

    public async Task<List<InventoryItemCardResponse>> GetListAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var spec = new SpecificationBuilder<RecentlyViewedProductEntity>()
            .AddCriteria(x => x.AccountId == accountId)
            .AddOrderBy(x => x.ViewedAtUtc, isDesc: true)
            .Take(5)
            .AddInclude(x => x.InventoryItem!)
            .AddInclude(x => x.InventoryItem!.Brand!)
            .AddInclude(x => x.InventoryItem!.Category!)
            .AddInclude(x => x.InventoryItem!.Images)
            .Build();

        var items = await _repository.GetAllBySpecAsync(spec, cancellationToken);

        return items
        .OrderByDescending(rv => rv.ViewedAtUtc)
         .Select(rv => _mapper.Map<InventoryItemCardResponse>(rv.InventoryItem))
         .ToList();
    }

    public async Task MergeGuestItemsAsync(Guid accountId, IEnumerable<MergeRecentlyViewedItem>? items, CancellationToken ct)
    {
        var safeItems = items ?? Enumerable.Empty<MergeRecentlyViewedItem>();

        var entities = safeItems.Select(i => new RecentlyViewedProductEntity
        {
            AccountId = accountId,
            InventoryItemId = i.InventoryItemId,
            ViewedAtUtc = i.ViewedAtUtc
        });

        await _repository.MergeAsync(accountId, entities, ct);
    }
    private async Task<RecentlyViewedProductEntity?> FindExistingAsync(Guid accountId, Guid inventoryItemId, CancellationToken cancellationToken)
    {
        var spec = new Specification<RecentlyViewedProductEntity>(
            x => x.AccountId == accountId && x.InventoryItemId == inventoryItemId);

        return await _repository.GetAsync(spec, cancellationToken);
    }

    private async Task UpdateTimestampAsync(RecentlyViewedProductEntity entity, CancellationToken cancellationToken)
    {
        entity.ViewedAtUtc = DateTime.UtcNow;
        await _repository.UpdateAsync(entity, cancellationToken);
    }

    private async Task InsertNewAsync(Guid accountId, Guid inventoryItemId, CancellationToken cancellationToken)
    {
        var entity = new RecentlyViewedProductEntity
        {
            AccountId = accountId,
            InventoryItemId = inventoryItemId,
            ViewedAtUtc = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);
    }

    private async Task EnforceMaxLimitAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var specAll = new SpecificationBuilder<RecentlyViewedProductEntity>()
            .AddCriteria(x => x.AccountId == accountId)
            .AddOrderBy(x => x.ViewedAtUtc, isDesc: true)
            .Build();

        var allItems = (await _repository.GetAllBySpecAsync(specAll, cancellationToken)).ToList();

        if (allItems.Count <= 5)
            return;

        var toRemoveIds = allItems
            .OrderByDescending(x => x.ViewedAtUtc)
            .Skip(5)
            .Select(x => x.InventoryItemId)
            .ToList();

        await _repository.DeleteManyBySpecAsync(
            new Specification<RecentlyViewedProductEntity>(x =>
                x.AccountId == accountId && toRemoveIds.Contains(x.InventoryItemId)),
            cancellationToken);
    }
}
