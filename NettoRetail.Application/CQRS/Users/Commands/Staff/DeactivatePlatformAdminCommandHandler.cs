using MediatR;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record DeactivatePlatfromAdminCommand(Guid AccountId, Guid CurrentAdminId) : IRequest;

public class DeactivatePlatformAdminCommandHandler(IStaffRepository accountRepository, ISessionTerminationService sessionTerminationService) : IRequestHandler<DeactivatePlatfromAdminCommand>
{
    public async Task Handle(DeactivatePlatfromAdminCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.AccountId);
        specification.AddInclude(adm => adm.Role);
        specification.AddCriteria(adm => adm.Role.Name.StartsWith("PlatformAdmin"));
        var admin = await accountRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("Platform admin not found.");

        if (admin.Id == request.CurrentAdminId)
        {
            throw new BadRequestException("You cannot deactivate yourself.");
        }

        if (admin.Status == StaffStatusEnum.Pending)
        {
            throw new BadRequestException("This platform admin has a Pending status.");
        }

        if (admin.Status == StaffStatusEnum.CreatePassword)
        {
            throw new BadRequestException("This platform admin has a CreatePassword staff status.");
        }

        admin.Status = admin.Status == StaffStatusEnum.Deactivated ? throw new BadRequestException("This platform admin already deactivated") : StaffStatusEnum.Deactivated;

        await sessionTerminationService.TerminateSessionsAsync(admin.Id, cancellationToken);

        await accountRepository.UpdateAsync(admin, cancellationToken);
    }
}