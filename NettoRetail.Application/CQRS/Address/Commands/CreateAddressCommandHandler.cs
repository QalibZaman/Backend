using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request.Address;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Address.Commands;

public record CreateAddressCommand(Guid UserId, CreateAddressRequest CreateAddressRequest) : IRequest<Unit>;
public class CreateAddressCommandHandler(IAddressRepository addressRepository, IMapper mapper) : IRequestHandler<CreateAddressCommand, Unit>
{
    public async Task<Unit> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        var addressEntity = mapper.Map<AddressEntity>(request.CreateAddressRequest);
        addressEntity.UserId = request.UserId;

        await addressRepository.AddAsync(addressEntity, cancellationToken);

        return Unit.Value;
    }
}
