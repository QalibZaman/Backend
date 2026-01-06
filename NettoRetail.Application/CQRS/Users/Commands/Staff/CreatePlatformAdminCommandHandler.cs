using Mapster;
using MediatR;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IRoleService;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record CreatePlatformAdminCommand(
    CreatePlatformAdminRequest Model,
    Guid SuperAdminId) : IRequest<CreatePlatformAdminResponse>;

public class CreatePlatformAdminCommandHandler(IAccountRepository accountRepo,
    IUnitOfWork unitOfWork,
    IRoleService roleService)
        : IRequestHandler<CreatePlatformAdminCommand, CreatePlatformAdminResponse>
{
    public async Task<CreatePlatformAdminResponse> Handle(
        CreatePlatformAdminCommand request,
        CancellationToken cancellationToken)
    {
        var model = new CreatePlatformAdminModel(request.Model);
        var role = await roleService.CreatePlatformAdminRole(model, cancellationToken);
        var account = model.Adapt<AccountEntity>();
        account.Role = role;
        using var tx = unitOfWork.BeginTransaction();
        await accountRepo.AddAsync(account, cancellationToken);
        tx.Commit();

        return new CreatePlatformAdminResponse
        {
            AccountId = account.Id,
            Email = account.Email,
            Status = account.Status,
            Permissions = model.Permissions
        };
    }
}
