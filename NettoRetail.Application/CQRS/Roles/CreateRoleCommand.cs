using MediatR;

namespace NettoRetail.Application.CQRS.Roles;

public record CreateRoleCommand() : IRequest;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand>
{
    public Task Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
