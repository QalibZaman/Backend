using Mapster;
using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record CreateInventoryItemCommand(AddInventoryItemModel Model, int ShopId) : IRequest<InventoryItemResponse>;

public class CreateInventoryItemCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : IRequestHandler<CreateInventoryItemCommand, InventoryItemResponse>
{
    public async Task<InventoryItemResponse> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Model.Description))
        {
            request.Model.Description = request.Model.Description.Trim();
        }

        var (categoryName, brandName) = await CheckForConflictsAsync(request, cancellationToken);

        using var transaction = unitOfWork.BeginTransaction();

        var response = await AddItemAndDetailsAsync(request, categoryName, brandName, cancellationToken);

        transaction.Commit();
        return response;
    }

    public async Task<(string CategoryName, string BrandName)> CheckForConflictsAsync(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var productByUpcSpecification = new GetProductByUpcSpecification(request.Model.ProductUpc);
        var productExists = await inventoryItemRepository.Exists(productByUpcSpecification, cancellationToken);

        if (productExists)
            throw new ConflictException("This product UPC is already registered.");

        var categoryName = await categoryRepository.GetCategoryNameAsync(request.Model.CategoryId, request.ShopId, cancellationToken)
            ?? throw new NotFoundException($"There is no category with Id: {request.Model.CategoryId}");

        var brandName = await brandRepository.GetBrandNameAsync(request.Model.BrandId, request.ShopId, cancellationToken)
            ?? throw new NotFoundException($"There is no brand with Id: {request.Model.BrandId}");

        return (categoryName, brandName);
    }

    private async Task<InventoryItemResponse> AddItemAndDetailsAsync(CreateInventoryItemCommand request, string categoryName, string brandName, CancellationToken cancellationToken)
    {
        var inventoryItem = mapper.Map<InventoryItemEntity>(request.Model);
        inventoryItem.ShopId = request.ShopId;

        inventoryItem.ProductDetails = mapper.Map<List<ProductDetailEntity>>(request.Model.ProductDetails);

        var responseModel = await inventoryItemRepository.AddAsync(inventoryItem, cancellationToken);

        var response = mapper.Map<InventoryItemResponse>(responseModel);
        response.ProductCategory = categoryName;
        response.ProductBrand = brandName;

        return response;
    }
}
