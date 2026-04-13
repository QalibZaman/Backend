using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record DeleteProfileImageCommand(Guid Id) : IRequest;

public class DeleteProfileImageCommandHandler(IStaffRepository accountRepository,
    IUnitOfWork unitOfWork,
    IAzureBlobService azureBlobService) : IRequestHandler<DeleteProfileImageCommand>
{
    public async Task Handle(DeleteProfileImageCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.Id);
        var account = await accountRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("No user with such id");

        if (string.IsNullOrEmpty(account.ProfileImageUrl))
            throw new BadRequestException("Profile image does not exist.");

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            await azureBlobService.DeleteByUrlAsync(account.ProfileImageUrl);

            account.ProfileImageUrl = null;
            await accountRepository.UpdateAsync(account, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}