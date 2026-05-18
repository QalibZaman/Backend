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

public record SetDeliveryMethodCommand(Guid CustomerId, int ShopId, SetDeliveryMethodRequest Request) : IRequest<DeliveryResponse>;

public class SetDeliveryMethodCommandHandler(
    IDeliveryRepository deliveryRepository,
    ICustomerRepository customerRepository,
    IAddressRepository addressRepository,
    ICartRepository cartRepository,
    IShopRepository shopRepository,
    IMapper mapper) : IRequestHandler<SetDeliveryMethodCommand, DeliveryResponse>
{
    public async Task<DeliveryResponse> Handle(SetDeliveryMethodCommand request, CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(request.CustomerId, cancellationToken);

        var shop = await shopRepository.GetAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Shop.ShopNotFoundMessage, request.ShopId));

        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        var delivery = await deliveryRepository.GetAsync(new GetDeliveryByCustomerIdSpecification(request.CustomerId), cancellationToken)
            ?? throw new NotFoundException("Delivery was not created for the current customer.");

        SetDeliveryMethodMappingModel mappingModel;

        switch (request.Request.DeliveryType)
        {
            case OrderDeliveryTypeEnum.InShop:
                mappingModel = new SetDeliveryMethodMappingModel
                {
                    DeliveryType = OrderDeliveryTypeEnum.InShop,
                    AddressId = null,
                    DeliveryFee = 0m,
                    PaymentTotal = CalculatePaymentTotal(cart, 0m),
                };
                break;
            case OrderDeliveryTypeEnum.Courier:
                var deliveryFee = shop.StandardDeliveryFee;
                mappingModel = new SetDeliveryMethodMappingModel
                {
                    DeliveryType = OrderDeliveryTypeEnum.Courier,
                    AddressId = delivery.AddressId ?? await GetDefaultAddressIdAsync(request.CustomerId, cancellationToken),
                    DeliveryFee = deliveryFee,
                    PaymentTotal = CalculatePaymentTotal(cart, deliveryFee),
                };
                break;
            default:
                throw new BadRequestException(ValidationErrorMessages.Order.InvalidDelivery);
        }

        mapper.Map(mappingModel, delivery);

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
}
