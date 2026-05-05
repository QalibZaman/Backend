using MediatR;
using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Delivery;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Delivery;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Delivery.Commands;

public record CreateCurrentDeliveryCommand(Guid CustomerId) : IRequest<DeliveryResponse>;

public class CreateCurrentDeliveryCommandHandler(
    IDeliveryRepository deliveryRepository,
    ICustomerRepository customerRepository,
    ICartRepository cartRepository,
    IMapper mapper) : IRequestHandler<CreateCurrentDeliveryCommand, DeliveryResponse>
{
    public async Task<DeliveryResponse> Handle(CreateCurrentDeliveryCommand request, CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(request.CustomerId, cancellationToken);

        var existingDelivery = await deliveryRepository.GetAsync(
            new GetDeliveryByCustomerIdSpecification(request.CustomerId),
            cancellationToken);

        if (existingDelivery != null)
        {
            return mapper.Map<DeliveryResponse>(existingDelivery);
        }

        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        var delivery = new DeliveryEntity
        {
            CustomerId = request.CustomerId,
            DeliveryFee = 0m,
            PaymentTotal = CalculatePaymentTotal(cart, 0m)
        };

        delivery = await deliveryRepository.AddAsync(delivery, cancellationToken);

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
