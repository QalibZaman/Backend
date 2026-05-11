using MediatR;
using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Delivery;
using NettoRetail.DataAccess.Specifications.Delivery;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Delivery.Queries;

public record GetCurrentDeliveryQuery(Guid CustomerId) : IRequest<DeliveryResponse>;

public class GetCurrentDeliveryQueryHandler(
    IDeliveryRepository deliveryRepository,
    IMapper mapper) : IRequestHandler<GetCurrentDeliveryQuery, DeliveryResponse>
{
    public async Task<DeliveryResponse> Handle(GetCurrentDeliveryQuery request, CancellationToken cancellationToken)
    {
        var delivery = await deliveryRepository.GetAsync(new GetDeliveryByCustomerIdSpecification(request.CustomerId), cancellationToken)
            ?? throw new NotFoundException("Delivery was not created for the current customer.");

        return mapper.Map<DeliveryResponse>(delivery);
    }
}
