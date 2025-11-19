using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.UOW;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record CreateInventoryItemCommand(AddInventoryItemModel Model, int ShopId) : IRequest<InventoryItemResponse>;

public class CreateInventoryItemCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICategoryRepository categoryRepository,
    IBrandRepository brandRepository,
    IProductDetailRepository productDetailRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateInventoryItemCommand, InventoryItemResponse>
{
    public async Task<InventoryItemResponse> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var productByUpcSpecification = new GetProductByUpcSpecification(request.Model.ProductUpc);
        var productExists = await inventoryItemRepository.Exists(productByUpcSpecification, cancellationToken);

        if (productExists)
            throw new ConflictException("This product UPC is already registered.");

        var categoryByIdSpecification = new GetCategoryByIdAndShopIdSpecification(request.Model.CategoryId, request.ShopId);
        var category = await categoryRepository.GetAsync(categoryByIdSpecification, cancellationToken)
            ?? throw new NotFoundException($"There is no category with Id: {request.Model.CategoryId}");

        var brandByIdSpecification = new GetBrandsByIdAndShopIdSpecification(request.Model.BrandId, request.ShopId);
        var brand = await brandRepository.GetAsync(brandByIdSpecification, cancellationToken)
            ?? throw new NotFoundException($"There is no brand with Id: {request.Model.BrandId}");

        var inventoryItem = request.Model.Adapt<InventoryItemEntity>();
        inventoryItem.ShopId = request.ShopId;
        var productDetails = request.Model.ProductDetails.Adapt<List<ProductDetailEntity>>();

        foreach (var detail in productDetails)
        {
            detail.InventoryItemId = inventoryItem.Id;
        }

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            var responseModel = await inventoryItemRepository.AddAsync(inventoryItem, cancellationToken);
            await productDetailRepository.AddRangeAsync(productDetails, cancellationToken);

            var response = responseModel.Adapt<InventoryItemResponse>();
            response.ProductCategory = category.Name;
            response.ProductBrand = brand.Name;

            transaction.Commit();

            return response;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}