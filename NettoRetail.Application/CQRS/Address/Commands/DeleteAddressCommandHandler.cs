using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Address;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Address.Commands;

public record DeleteAddressCommand(Guid Id, Guid UserId) : IRequest<Unit>;

public class DeleteAddressCommandHandler(IAddressRepository addressRepository, IUnitOfWork unitOfWork) : IRequestHandler<DeleteAddressCommand, Unit>
{
    public async Task<Unit> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAddressByIdAndUserIdSpecification(request.Id, request.UserId);
        var address = await addressRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Address.AddressNotFoundMessage);

        using var transaction = unitOfWork.BeginTransaction();

        if (address.IsDefault)
        {
            await ChangeDefaultAddress(request.UserId, address.Id, cancellationToken);
        }

        await addressRepository.DeleteAsync(address, cancellationToken);

        transaction.Commit();

        return Unit.Value;
    }

    private async Task ChangeDefaultAddress(Guid userId, Guid excludedAddressId, CancellationToken cancellationToken)
    {
        var fallbackAddress = await addressRepository.GetFallbackAddressAsync(userId, excludedAddressId, cancellationToken);

        if (fallbackAddress != null)
        {
            fallbackAddress.IsDefault = true;
            await addressRepository.UpdateAsync(fallbackAddress, cancellationToken);
        }
    }
}
