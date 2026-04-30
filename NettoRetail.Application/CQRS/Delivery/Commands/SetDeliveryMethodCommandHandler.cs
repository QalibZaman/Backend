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

public record SetDeliveryMethodCommand(Guid CustomerId, int ShopId, SetDeliveryMethodRequest Request) : IRequest<DeliveryResponse>;

public class SetDeliveryMethodCommandHandler(
    IDeliveryRepository deliveryRepository,
    ICustomerRepository customerRepository,
    IAddressRepository addressRepository,
    ICartRepository cartRepository,
    IShopRepository shopRepository) : IRequestHandler<SetDeliveryMethodCommand, DeliveryResponse>
{
    public async Task<DeliveryResponse> Handle(SetDeliveryMethodCommand request, CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(request.CustomerId, cancellationToken);

        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Shop.ShopNotFoundMessage, request.ShopId));

        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        var delivery = await deliveryRepository.GetAsync(new GetDeliveryByCustomerIdSpecification(request.CustomerId), cancellationToken);
        var isNew = delivery == null;

        delivery ??= new DeliveryEntity
        {
            CustomerId = request.CustomerId
        };

        switch (request.Request.DeliveryType)
        {
            case OrderDeliveryTypeEnum.InShop:
                delivery.DeliveryType = OrderDeliveryTypeEnum.InShop;
                delivery.AddressId = null;
                delivery.DeliveryFee = 0m;
                break;
            case OrderDeliveryTypeEnum.Courier:
                delivery.DeliveryType = OrderDeliveryTypeEnum.Courier;
                delivery.AddressId ??= await GetDefaultAddressIdAsync(request.CustomerId, cancellationToken);
                delivery.DeliveryFee = shop.StandardDeliveryFee;
                break;
            default:
                throw new BadRequestException(ValidationErrorMessages.Order.InvalidDelivery);
        }

        delivery.PaymentTotal = CalculatePaymentTotal(cart, delivery.DeliveryFee);
        delivery.UpdatedAt = DateTime.UtcNow;

        delivery = isNew
            ? await deliveryRepository.AddAsync(delivery, cancellationToken)
            : await deliveryRepository.UpdateAsync(delivery, cancellationToken);

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

    private async Task<Guid?> GetDefaultAddressIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var addresses = await addressRepository.GetManyBySpecAsync(new GetAddressesByUserIdSpecification(customerId), cancellationToken);

        return addresses.FirstOrDefault(address => address.IsDefault)?.Id;
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
