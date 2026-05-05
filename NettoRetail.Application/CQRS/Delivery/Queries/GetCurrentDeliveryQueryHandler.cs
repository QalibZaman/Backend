using MediatR;
using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Delivery;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Delivery;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Delivery.Queries;

public record GetCurrentDeliveryQuery(Guid CustomerId) : IRequest<DeliveryResponse>;

public class GetCurrentDeliveryQueryHandler(
    IDeliveryRepository deliveryRepository,
    ICustomerRepository customerRepository,
    IMapper mapper) : IRequestHandler<GetCurrentDeliveryQuery, DeliveryResponse>
{
    public async Task<DeliveryResponse> Handle(GetCurrentDeliveryQuery request, CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(request.CustomerId, cancellationToken);

        var delivery = await deliveryRepository.GetAsync(new GetDeliveryByCustomerIdSpecification(request.CustomerId), cancellationToken)
            ?? throw new NotFoundException("Delivery was not created for the current customer.");

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
}
