using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Delivery;
using NettoRetail.DataAccess.Specifications.Address;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Delivery;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Delivery.Commands;

public record SetDeliveryAddressCommand(Guid CustomerId, int ShopId, SetDeliveryAddressRequest Request) : IRequest<DeliveryResponse>;

public class SetDeliveryAddressCommandHandler(
    IDeliveryRepository deliveryRepository,
    ICustomerRepository customerRepository,
    IAddressRepository addressRepository,
    ICartRepository cartRepository,
    IShopRepository shopRepository) : IRequestHandler<SetDeliveryAddressCommand, DeliveryResponse>
{
    public async Task<DeliveryResponse> Handle(SetDeliveryAddressCommand request, CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(request.CustomerId, cancellationToken);

        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Shop.ShopNotFoundMessage, request.ShopId));

        var address = await addressRepository.GetAsync(
            new GetAddressByIdAndUserIdSpecification(request.Request.AddressId, request.CustomerId),
            cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Address.AddressNotFoundMessage);

        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        var delivery = await deliveryRepository.GetAsync(new GetDeliveryByCustomerIdSpecification(request.CustomerId), cancellationToken)
            ?? throw new BadRequestException("Courier delivery method must be selected before setting an address.");

        if (delivery.DeliveryType != OrderDeliveryTypeEnum.Courier)
        {
            throw new BadRequestException("Courier delivery method must be selected before setting an address.");
        }

        delivery.AddressId = address.Id;
        delivery.DeliveryFee = shop.StandardDeliveryFee;
        delivery.PaymentTotal = CalculatePaymentTotal(cart, delivery.DeliveryFee);
        delivery.UpdatedAt = DateTime.UtcNow;

        delivery = await deliveryRepository.UpdateAsync(delivery, cancellationToken);

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
