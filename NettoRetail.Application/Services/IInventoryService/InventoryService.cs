using Mapster;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IInventoryService;

public class InventoryService : IInventoryService
{
    private readonly InventoryItemOptions _options;
    private readonly IInventoryItemRepository _inventoryItemRepository;

    public InventoryService(IOptions<InventoryItemOptions> options, IInventoryItemRepository inventoryItemRepository)
    {
        _options = options.Value;
        _inventoryItemRepository = inventoryItemRepository;
    }

    public bool IsItemNew(InventoryItemEntity item) => item.LastUpdatedDate >= DateTime.UtcNow.AddDays(-_options.NewItemDaysThreshold);
    public bool IsItemOutOfStock(InventoryItemEntity item) => item.AvailableAmount <= 0;
    public bool IsItemOnSale(InventoryItemEntity item) => item.PromotionPrice.HasValue;

    public async Task<PaginatedResponse<InventoryItemListResponse>> GetShopItemsBySearchAsync(string query, int shopId,
        PaginationRequest pageModel, CancellationToken cancellationToken)
    {
        pageModel ??= new PaginationRequest();

        var InventoryItemSpecBuilder = new SpecificationBuilder<InventoryItemEntity>();
        InventoryItemSpecBuilder
            .Add<GetInventoryItemsByPartialNameSpecification>(query)
            .Add<GetInventoryItemsByShopIdSpecification>(shopId)
            .AddInclude(x => x.Brand)
            .AddInclude(x => x.Category);

        var specification = InventoryItemSpecBuilder.Build();
        var products = await _inventoryItemRepository.GetAllBySpecAsync(specification, cancellationToken);

        var total = products.Count();

        var pagedProduct = products
          .Skip((pageModel.PageNumber - 1) * pageModel.PageSize)
          .Take(pageModel.PageSize)
          .ToList();

        var productResponses = pagedProduct.Adapt<List<InventoryItemListResponse>>();

        var response = new PaginatedResponse<InventoryItemListResponse>
        {
            Items = productResponses,
            TotalAmount = total,
        };

        return response;
    }
}
