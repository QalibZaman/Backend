using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record ActivatePlatfromAdminCommand(Guid Id, Guid CurrentAdminId) : IRequest;

public class ActivatePlatformAdminCommandHandler(IStaffRepository accountRepository) : IRequestHandler<ActivatePlatfromAdminCommand>
{
    public async Task Handle(ActivatePlatfromAdminCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.Id);
        specification.AddInclude(adm => adm.Role);
        specification.AddCriteria(adm => adm.Role.Name.StartsWith("PlatformAdmin"));
        var admin = await accountRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("Platform admin not found.");

        if (admin.Id == request.CurrentAdminId)
        {
            throw new BadRequestException("You cannot activate yourself.");
        }

        if (admin.Status == StaffStatusEnum.Pending)
        {
            throw new BadRequestException("This platform admin has a pending status.");
        }

        admin.Status = admin.Status != StaffStatusEnum.Deactivated ? throw new BadRequestException("This platform admin already activated") : StaffStatusEnum.Active;

        await accountRepository.UpdateAsync(admin, cancellationToken);
    }
}