using MediatR;
using MapsterMapper;
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
    IShopRepository shopRepository,
    IMapper mapper) : IRequestHandler<SetDeliveryAddressCommand, DeliveryResponse>
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
            ?? throw new BadRequestException(ValidationErrorMessages.Delivery.MethodSelectionObligatory);

        if (delivery.DeliveryType != OrderDeliveryTypeEnum.Courier)
        {
            throw new BadRequestException(ValidationErrorMessages.Delivery.MethodSelectionObligatory);
        }

        var deliveryFee = shop.StandardDeliveryFee;
        mapper.Map(
            new SetDeliveryAddressMappingModel
            {
                AddressId = address.Id,
                DeliveryFee = deliveryFee,
                PaymentTotal = CalculatePaymentTotal(cart, deliveryFee),
            },
            delivery);

        delivery = await deliveryRepository.UpdateAsync(delivery, cancellationToken);

        return mapper.Map<DeliveryResponse>(delivery);
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
}
