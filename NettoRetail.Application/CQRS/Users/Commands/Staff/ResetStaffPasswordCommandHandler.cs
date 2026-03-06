using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;
public record ResetStaffPasswordCommand(Guid Id, int ShopId) : IRequest;

public class ResetStaffPasswordCommandHandler(
    IStaffRepository accountRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService)
    : IRequestHandler<ResetStaffPasswordCommand>
{
    public async Task Handle(ResetStaffPasswordCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetStaffByIdSpecification(request.Id);
        var staff = await accountRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException(StaffConstants.StaffMemberNotFound);

        if (staff.ShopId != request.ShopId)
            throw new ForbiddenException(StaffConstants.StaffMemberAccessDenied);

        var emailModel = await emailBuilderService.BuildResetPasswordEmailAsync(staff.Email, staff.FirstName);
        await emailSender.SendAsync(emailModel, staff.Id);
    }
}
