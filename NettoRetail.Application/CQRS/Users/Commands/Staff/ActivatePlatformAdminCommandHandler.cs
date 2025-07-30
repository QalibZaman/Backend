using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.PlatformAdmin;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record ActivatePlatfromAdminCommand(Guid id, Guid currentAdminId) : IRequest;

public class ActivatePlatformAdminCommandHandler(IPlatformAdminRepository platformAdminRepository) : IRequestHandler<ActivatePlatfromAdminCommand>
{
    public async Task Handle(ActivatePlatfromAdminCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetPlatformAdminByIdSpecification(request.id);
        specification.AddInclude(adm => adm.Account);
        var admin = await platformAdminRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("Platform admin not found.");

        if (admin.AccountId == request.currentAdminId)
        {
            throw new BadRequestException("You cannot activate yourself.");
        }

        if (admin.PlatformAdminStatus == PlatformAdminStatusEnum.Pending)
        {
            throw new BadRequestException("This platform admin has a pending status.");
        }

        admin.PlatformAdminStatus = admin.PlatformAdminStatus == PlatformAdminStatusEnum.Active ? throw new BadRequestException("This platform admin already activated") : PlatformAdminStatusEnum.Active;

        await platformAdminRepository.UpdateAsync(admin, cancellationToken);
    }
}