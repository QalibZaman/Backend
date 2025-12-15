using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NettoRetail.Application.CQRS.InventoryItems.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IInventoryService;

public class InventoryService(
    IOptions<InventoryItemOptions> options,
    IInventoryItemRepository inventoryItemRepository,
    ICsvFileService csvFileService,
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository,
    ILogger<InventoryService> logger) : IInventoryService
{
    private readonly InventoryItemOptions _options = options.Value;
    private readonly IInventoryItemRepository _inventoryItemRepository = inventoryItemRepository;
    private readonly ICsvFileService _csvFileService = csvFileService;
    private readonly ILogger<InventoryService> _logger = logger;

    public bool IsItemNew(InventoryItemEntity item) =>
        item.LastUpdatedDate >= DateTime.UtcNow.AddDays(-_options.NewItemDaysThreshold);

    public bool IsItemOutOfStock(InventoryItemEntity item) =>
        item.AvailableAmount <= 0;

    public bool IsItemOnSale(InventoryItemEntity item) =>
        item.PromotionPrice.HasValue;

    public async Task<PaginatedResponse<InventoryItemListResponse>> GetShopItemsBySearchAsync(
        string query, int shopId, PaginationRequest pageModel, CancellationToken cancellationToken)
    {
        pageModel ??= new PaginationRequest();

        var inventoryItemSpecBuilder = new SpecificationBuilder<InventoryItemEntity>();
        inventoryItemSpecBuilder
            .Add<GetInventoryItemsByPartialNameSpecification>(query)
            .Add<GetInventoryItemsByShopIdSpecification>(shopId)
            .AddInclude(x => x.Brand)
            .AddInclude(x => x.Category);

        var specification = inventoryItemSpecBuilder.Build();
        var products = await _inventoryItemRepository.GetAllBySpecAsync(specification, cancellationToken);

        var total = products.Count();

        var pagedProduct = products
            .Skip((pageModel.PageNumber - 1) * pageModel.PageSize)
            .Take(pageModel.PageSize)
            .ToList();

        var productResponses = pagedProduct.Adapt<List<InventoryItemListResponse>>();

        return new PaginatedResponse<InventoryItemListResponse>
        {
            Items = productResponses,
            TotalAmount = total,
        };
    }

    public async Task<byte[]> DownloadInventoryItemsCsvAsync(int userShopId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting inventory items CSV download for ShopId: {ShopId}", userShopId);

        var itemByRelatedCategorySpecification = new GetInventoryItemsRelatedCategories(userShopId);
        itemByRelatedCategorySpecification.AddInclude(i => i.Brand!);

        var inventoryItems = await _inventoryItemRepository.GetAllBySpecAsync(itemByRelatedCategorySpecification, cancellationToken);

        if (!inventoryItems.Any())
        {
            _logger.LogWarning("No inventory items found for ShopId: {ShopId}", userShopId);
            throw new NotFoundException("There are no items in the list.");
        }

        var inventoryItemsData = await _csvFileService.GetEntitiesCsvData<InventoryItemCsvModel, InventoryItemEntity>(
            inventoryItems, cancellationToken);

        _logger.LogInformation("Successfully generated CSV for {Count} items (ShopId: {ShopId})", inventoryItems.Count(), userShopId);

        return inventoryItemsData;
    }
    
    public  async Task ValidateRequestModel(UpdateInventoryItemCommand request, InventoryItemEntity inventoryItem, CancellationToken cancellationToken)
    {
        if (request.Model.ProductUpc.HasValue && inventoryItem.ProductUpc != request.Model.ProductUpc)
        {
            var productByNameSpecification = new GetProductByUpcSpecification(request.Model.ProductUpc.Value);
            var productExists = await inventoryItemRepository.Exists(productByNameSpecification, cancellationToken);

            if (productExists)
                throw new ConflictException("This product UPC is already registered.");
        }

        if (request.Model.CategoryId != null)
        {
            var categoryByIdSpecification = new GetCategoryByIdAndShopIdSpecification(request.Model.CategoryId.Value, request.ShopId);
            var categoryExists = await categoryRepository.Exists(categoryByIdSpecification, cancellationToken);

            if (!categoryExists)
                throw new NotFoundException("There is no category with such id.");
        }

        if (request.Model.BrandId != null)
        {
            var brandByIdSpecification = new GetBrandsByIdAndShopIdSpecification(request.Model.BrandId.Value, request.ShopId);
            var brandExists = await brandRepository.Exists(brandByIdSpecification, cancellationToken);

            if (!brandExists)
                throw new NotFoundException("There is no brand with such id.");
        }

        if (request.Model.IsPromoted == true && request.Model.PromotionPrice == null)
        {
            throw new BadRequestException("Promotion price must be provided for promoted products.");
        }

        if (request.Model.IsPromoted == false && request.Model.PromotionPrice != null)
        {
            throw new BadRequestException("Promotion price cannot be provided for non-promoted products.");
        }

        if (request.Model.PromotionPrice != null && request.Model.PromotionPrice >= request.Model.Price)
        {
            throw new BadRequestException("Promotion price must be less than the regular price.");
        }
    }
}
