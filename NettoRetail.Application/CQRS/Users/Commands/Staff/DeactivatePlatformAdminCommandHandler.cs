using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.PlatformAdmin;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record DeactivatePlatfromAdminCommand(Guid id, Guid currentAdminId) : IRequest;

public class DeactivatePlatformAdminCommandHandler(IPlatformAdminRepository platformAdminRepository, ISessionTerminationService sessionTerminationService) : IRequestHandler<DeactivatePlatfromAdminCommand>
{
    public async Task Handle(DeactivatePlatfromAdminCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetPlatformAdminByIdSpecification(request.id);
        specification.AddInclude(adm => adm.Account);
        var admin = await platformAdminRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("Platform admin not found.");

        if (admin.AccountId == request.currentAdminId)
        {
            throw new BadRequestException("You cannot deactivate yourself.");
        }

        if (admin.PlatformAdminStatus == PlatformAdminStatusEnum.Pending)
        {
            throw new BadRequestException("This platform admin has a Pending status.");
        }

        if (admin.Account.Status == StaffStatusEnum.CreatePassword)
        {
            throw new BadRequestException("This platform admin has a CreatePassword staff status.");
        }

        admin.PlatformAdminStatus = admin.PlatformAdminStatus == PlatformAdminStatusEnum.Inactive ? throw new BadRequestException("This platform admin already deactivated") : PlatformAdminStatusEnum.Inactive;
        admin.Account.Status = StaffStatusEnum.Offline;

        await sessionTerminationService.TerminateSessionsAsync(admin.AccountId, cancellationToken);

        await platformAdminRepository.UpdateAsync(admin, cancellationToken);
    }
}