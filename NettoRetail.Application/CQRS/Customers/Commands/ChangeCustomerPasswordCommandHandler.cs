using MediatR;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Application.Services.IPasswordService;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record ChangeCustomerPasswordCommand(Guid UserId, ChangePasswordRequest Model) : IRequest<Unit>;

public class ChangeCustomerPasswordCommandHandler(IPasswordService passwordService)
    : IRequestHandler<ChangeCustomerPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangeCustomerPasswordCommand request, CancellationToken cancellationToken)
    {
        await passwordService.ChangeCustomerPasswordAsync(
            request.UserId,
            request.Model.CurrentPassword,
            request.Model.NewPassword,
            cancellationToken);

        return Unit.Value;
    }
}
