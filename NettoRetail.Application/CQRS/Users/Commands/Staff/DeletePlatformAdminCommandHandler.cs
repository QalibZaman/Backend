using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record DeletePlatformAdminCommand(Guid Id, Guid CurrentAdminId) : IRequest;

public class DeletePlatformAdminCommandHandler(
    IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeletePlatformAdminCommand>
{
    public async Task Handle(DeletePlatformAdminCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByIdSpecification(request.Id);
        specification.AddInclude(acc => acc.Role);
        specification.AddCriteria(acc => acc.Role.Name.StartsWith("PlatformAdmin"));

        if (request.Id == request.CurrentAdminId)
        {
            throw new BadRequestException("You cannot delete your own account.");
        }

        var admin = await accountRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("Platform admin not found.");

        if (admin.Status != StaffStatusEnum.Deactivated && admin.Status != StaffStatusEnum.Pending)
        {
            throw new BadRequestException("Only deactivated or pending platform admins can be deleted.");
        }

        using var tx = unitOfWork.BeginTransaction();
        await accountRepository.DeleteAsync(admin, cancellationToken);
        await roleRepository.DeleteAsync(admin.Role, cancellationToken);
        tx.Commit();
    }
}
