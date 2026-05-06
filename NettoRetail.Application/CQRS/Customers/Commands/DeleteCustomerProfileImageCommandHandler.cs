using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record DeleteCustomerProfileImageCommand(Guid UserId) : IRequest;
public class DeleteCustomerProfileImageCommandHandler(
    ICustomerRepository customerRepository,
    IAzureBlobService azureBlobService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCustomerProfileImageCommand>
{
    public async Task Handle(DeleteCustomerProfileImageCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetCustomerByIdSpecification(request.UserId);
        var account = await customerRepository.GetAsync(specification, cancellationToken)
            ?? throw new BadRequestException("Profile image not found.");

        using var transaction = unitOfWork.BeginTransaction();
        await azureBlobService.DeleteByUrlAsync(account.ProfileImageUrl);
        account.ProfileImageUrl = null;
        await customerRepository.UpdateAsync(account, cancellationToken);

        transaction.Commit();
    }
}
