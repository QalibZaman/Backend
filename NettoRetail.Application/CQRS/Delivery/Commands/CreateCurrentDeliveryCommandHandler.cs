using MediatR;
using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Delivery;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Delivery;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Delivery.Commands;

public record CreateCurrentDeliveryCommand(Guid CustomerId, int ShopId) : IRequest<DeliveryResponse>;

public class CreateCurrentDeliveryCommandHandler(
    IDeliveryRepository deliveryRepository,
    ICustomerRepository customerRepository,
    ICartRepository cartRepository,
    IShopRepository shopRepository,
    IMapper mapper) : IRequestHandler<CreateCurrentDeliveryCommand, DeliveryResponse>
{
    public async Task<DeliveryResponse> Handle(CreateCurrentDeliveryCommand request, CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(request.CustomerId, cancellationToken);

        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Shop.ShopNotFoundMessage, request.ShopId));

        var existingDelivery = await deliveryRepository.GetAsync(
            new GetDeliveryByCustomerIdSpecification(request.CustomerId),
            cancellationToken);

        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (existingDelivery != null)
        {
            var expectedFee = existingDelivery.DeliveryType == OrderDeliveryTypeEnum.Courier
                ? shop.StandardDeliveryFee
                : 0m;
            var expectedTotal = CalculatePaymentTotal(cart, expectedFee);

            if (existingDelivery.DeliveryFee != expectedFee || existingDelivery.PaymentTotal != expectedTotal)
            {
                existingDelivery.DeliveryFee = expectedFee;
                existingDelivery.PaymentTotal = expectedTotal;
                existingDelivery.UpdatedAt = DateTime.UtcNow;

                existingDelivery = await deliveryRepository.UpdateAsync(existingDelivery, cancellationToken);
            }

            return mapper.Map<DeliveryResponse>(existingDelivery);
        }

        var delivery = new DeliveryEntity
        {
            CustomerId = request.CustomerId,
            DeliveryFee = 0m,
            PaymentTotal = CalculatePaymentTotal(cart, 0m),
        };

        delivery = await deliveryRepository.AddAsync(delivery, cancellationToken);

        return mapper.Map<DeliveryResponse>(delivery);
    }

    private async Task EnsureCustomerExistsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var exists = await customerRepository.ExistsAsync(new GetCustomerByIdSpecification(customerId), cancellationToken);
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
}
