using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;
public record ResetStaffPasswordCommand(Guid Id, int ShopId) : IRequest;

public class ResetStaffPasswordCommandHandler(
    IAccountRepository accountRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService)
    : IRequestHandler<ResetStaffPasswordCommand>
{
    public async Task Handle(ResetStaffPasswordCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetAccountByIdSpecification(request.Id);
        var staff = await accountRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Staff member not found.");

        if (staff.ShopId != request.ShopId)
            throw new BadRequestException("You are not allowed to access this staff member.");

        var emailModel = await emailBuilderService.BuildResetPasswordEmailAsync(staff.Email, staff.FirstName);
        await emailSender.SendAsync(emailModel);
    }
}
