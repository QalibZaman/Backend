using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.Specifications.ProductReview;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;


namespace NettoRetail.Application.Services.IProductReviewService
{
    public class ProductReviewService(
        IProductReviewRepository productReviewRepository,
        IInventoryItemRepository inventoryItemRepository,
        IOrderRepository orderRepository,
        IAzureBlobService azureBlobService,
        IMapper mapper
        ) : IProductReviewService
    {
        public async Task CreateProductReviewAsync(CreateProductReviewRequest request, Guid customerId, Guid inventoryItemId, CancellationToken cancellationToken)
        {
            var inventoryItem = await inventoryItemRepository.GetAsync(
            new GetInventoryItemByIdSpecification(inventoryItemId),
            cancellationToken)
            ?? throw new NotFoundException("Product not found.");

            var hasCustomerReviewedProductSpecification = new HasCustomerReviewedProductSpecification(customerId, inventoryItemId);
            var alreadyReviewed = await productReviewRepository.ExistsAsync(hasCustomerReviewedProductSpecification, cancellationToken);
            if (alreadyReviewed)
                throw new ConflictException("You have already reviewed this product.");

            var isVerifiedPurchaseSpecification = new IsVerifiedPurchaseSpecification(customerId, inventoryItemId);
            var isVerifiedPurchase = await orderRepository.ExistsAsync(isVerifiedPurchaseSpecification, cancellationToken);

            if (!isVerifiedPurchase)
                throw new BadRequestException("You can only review products you have purchased.");

            if (inventoryItem.HasVariations && string.IsNullOrEmpty(request.VariantInfo))
                throw new BadRequestException(" Variant selection is required for this product.");

            var entity = mapper.Map<ProductReviewEntity>(request);
            entity.Id = Guid.NewGuid();
            entity.InventoryItemId = inventoryItemId;
            entity.CustomerId = customerId;
            entity.InventoryItemId = inventoryItemId;
            entity.DateSubmitted = DateTime.UtcNow;

            await productReviewRepository.AddAsync(entity, cancellationToken);
        }
    }
}
