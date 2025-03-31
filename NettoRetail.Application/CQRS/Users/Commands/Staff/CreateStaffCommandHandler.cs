using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record CreateStaffCommand(CreateStaffRequest Model, int ShopId) : IRequest<CreateStaffResponse>;
public class CreateStaffCommandHandler(
    IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender)
    : IRequestHandler<CreateStaffCommand, CreateStaffResponse>
{
    public async Task<CreateStaffResponse> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var accountSpecification = new GetAccountByEmailSpecification(request.Model.Email);
        var existingAccount = await accountRepository
            .GetAsync(accountSpecification, cancellationToken);

        if (existingAccount != null)
            throw new ConflictException("An account with this email already exists.");

        var roleSpecification = new GetRoleByIdSpecification(request.Model.RoleId, request.ShopId);
        var role = await roleRepository.GetAsync(roleSpecification, cancellationToken)
            ?? throw new NotFoundException("Role not found");

        var account = request.Model.Adapt<AccountEntity>();
        account.RoleId = role.Id;
        account.Status = StaffStatusEnum.CreatePassword;

        var initialPassword = Guid.NewGuid().ToString();
        account.PasswordHash = PasswordHasher.HashPassword(initialPassword);

        var createdAccount = await accountRepository.AddAsync(account, cancellationToken);

        var response = createdAccount.Adapt<CreateStaffResponse>();

        var emailModel = await emailBuilderService
            .BuildResetPasswordEmailAsync(request.Model.Email, request.Model.FirstName);

        await emailSender.SendAsync(emailModel);

        return response;
    }
}
