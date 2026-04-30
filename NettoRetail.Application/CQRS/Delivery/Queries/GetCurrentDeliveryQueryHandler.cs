using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Delivery;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Delivery;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Delivery.Queries;

public record GetCurrentDeliveryQuery(Guid CustomerId, int ShopId) : IRequest<DeliveryResponse>;

public class GetCurrentDeliveryQueryHandler(
    IDeliveryRepository deliveryRepository,
    ICustomerRepository customerRepository,
    ICartRepository cartRepository,
    IShopRepository shopRepository) : IRequestHandler<GetCurrentDeliveryQuery, DeliveryResponse>
{
    public async Task<DeliveryResponse> Handle(GetCurrentDeliveryQuery request, CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(request.CustomerId, cancellationToken);

        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Shop.ShopNotFoundMessage, request.ShopId));

        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        var delivery = await deliveryRepository.GetAsync(new GetDeliveryByCustomerIdSpecification(request.CustomerId), cancellationToken);

        if (delivery == null)
        {
            delivery = new DeliveryEntity
            {
                CustomerId = request.CustomerId,
                DeliveryFee = 0m,
                PaymentTotal = CalculatePaymentTotal(cart, 0m)
            };

            delivery = await deliveryRepository.AddAsync(delivery, cancellationToken);
        }
        else
        {
            var expectedFee = delivery.DeliveryType == OrderDeliveryTypeEnum.Courier
                ? shop.StandardDeliveryFee
                : 0m;
            var expectedTotal = CalculatePaymentTotal(cart, expectedFee);

            if (delivery.DeliveryFee != expectedFee || delivery.PaymentTotal != expectedTotal)
            {
                delivery.DeliveryFee = expectedFee;
                delivery.PaymentTotal = expectedTotal;
                delivery.UpdatedAt = DateTime.UtcNow;
                delivery = await deliveryRepository.UpdateAsync(delivery, cancellationToken);
            }
        }

        return MapResponse(delivery);
    }

    private async Task EnsureCustomerExistsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var exists = await customerRepository.Exists(new GetCustomerByIdSpecification(customerId), cancellationToken);
        if (!exists)
        {
            throw new NotFoundException(ValidationErrorMessages.Customer.CustomerNotFoundErrorMessage);
        }
    }

    private static decimal CalculatePaymentTotal(CartEntity? cart, decimal deliveryFee)
    {
        var subtotal = cart?.Items.Sum(item => item.Price * item.Quantity) ?? 0m;
        var totalSale = cart?.Items.Sum(item => (item.Price - item.GetEffectivePrice()) * item.Quantity) ?? 0m;

        return subtotal + deliveryFee - totalSale;
    }

    private static DeliveryResponse MapResponse(DeliveryEntity delivery)
    {
        return new DeliveryResponse
        {
            CustomerId = delivery.CustomerId,
            OrderId = delivery.OrderId,
            AddressId = delivery.AddressId,
            DeliveryType = delivery.DeliveryType,
            DeliveryFee = delivery.DeliveryFee,
            PaymentTotal = delivery.PaymentTotal,
            CreatedAt = delivery.CreatedAt,
            UpdatedAt = delivery.UpdatedAt
        };
    }
}
