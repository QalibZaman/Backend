using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.Address;
using NettoRetail.DataAccess.Specifications.Address;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Address.Queries;

public record GetAddressByIdAndUserIdQuery(Guid Id, Guid UserId) : IRequest<AddressResponse>;
public class GetAddressByIdAndUserIdQueryHandler(IAddressRepository addressRepository, IMapper mapper) : IRequestHandler<GetAddressByIdAndUserIdQuery, AddressResponse>
{
    public async Task<AddressResponse> Handle(GetAddressByIdAndUserIdQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetAddressByIdAndUserIdSpecification(request.Id, request.UserId);
        var address = await addressRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Address.AddressNotFoundMessage);

        var addressResponse = mapper.Map<AddressResponse>(address);

        return addressResponse;
    }
}
