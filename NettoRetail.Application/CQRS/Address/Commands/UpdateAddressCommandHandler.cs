using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Address;
using NettoRetail.DataAccess.Specifications.Address;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Address.Commands;

public record UpdateAddressCommand(Guid Id, Guid UserId, UpdateAddressRequest UpdateAddress) : IRequest<Unit>;
public class UpdateAddressCommandHandler(IAddressRepository addressRepository, IMapper mapper, IUnitOfWork unitOfWork) : IRequestHandler<UpdateAddressCommand, Unit>
{
    public async Task<Unit> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAddressByIdAndUserIdSpecification(request.Id, request.UserId);
        var address = await addressRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Address.AddressNotFoundMessage);

        using var transaction = unitOfWork.BeginTransaction();

        if (request.UpdateAddress.IsDefault != address.IsDefault)
        {
            var allAddresses = address.User.Addresses.ToList();

            if (allAddresses.Count < 2)
                throw new BadRequestException(HandlerConstants.Address.AtLeastTwoAddressesRequiredMessage);

            var newDefaultAddress = address.Id;
            foreach (var entity in allAddresses)
            {
                entity.IsDefault = entity.Id == newDefaultAddress;
            }

            await addressRepository.UpdateManyAsync(allAddresses, cancellationToken);
        }

        mapper.Map(request.UpdateAddress, address);

        await addressRepository.UpdateAsync(address, cancellationToken);

        transaction.Commit();
        return Unit.Value;
    }
}
