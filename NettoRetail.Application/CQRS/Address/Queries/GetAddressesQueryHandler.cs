using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Response.Address;
using NettoRetail.DataAccess.Specifications.Address;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Address.Queries;

public record GetAddressesQuery(Guid UserId) : IRequest<IList<AddressResponse>>;

public class GetAddressesQueryHandler(IAddressRepository addressRepository, IMapper mapper) : IRequestHandler<GetAddressesQuery, IList<AddressResponse>>
{
    public async Task<IList<AddressResponse>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetAddressesByUserIdSpecification(request.UserId);
        var addresses = await addressRepository.GetManyBySpecAsync(specification, cancellationToken);

        var addressResponses = mapper.Map<IList<AddressResponse>>(addresses);

        return addressResponses;
    }
}
