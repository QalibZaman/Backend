using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record DeletePlatformAdminCommand(Guid Id, Guid CurrentAdminId) : IRequest;

public class DeletePlatformAdminCommandHandler(
    IStaffRepository accountRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeletePlatformAdminCommand>
{
    public async Task Handle(DeletePlatformAdminCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == request.CurrentAdminId)
        {
            throw new BadRequestException(HandlerConstants.Account.CannotDeleteOwnAccountMessage);
        }

        var specification = new GetPlatformAdminByIdSpecification(request.Id);

        var admin = await accountRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(string.Format(HandlerConstants.Account.AccountNotFoundMessage, request.Id));

        if (!(admin.Status == StaffStatusEnum.Deactivated || admin.Status == StaffStatusEnum.Pending))
        {
            throw new BadRequestException(HandlerConstants.Account.CannotDeletePlatformAdminMessage);
        }

        using var transaction = unitOfWork.BeginTransaction();
        await accountRepository.DeleteAsync(admin, cancellationToken);
        await roleRepository.DeleteAsync(admin.Role, cancellationToken);
        transaction.Commit();
    }
}
