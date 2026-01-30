using Mapster;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services.IStaffService;

public class StaffService(
    IAccountRepository accountRepository,
    IRoleRepository roleRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService,
    ISessionTerminationService sessionTerminationService) : IStaffService
{
    public async Task CreateStaffAsync(StaffCsvModel staffCsvModel, int shopId, CancellationToken cancelletionToken = default)
    {
        var accountSpecification = new GetAccountByEmailSpecification(staffCsvModel.Email);
        var existingAccount = await accountRepository
            .GetAsync(accountSpecification, cancelletionToken);

        if (existingAccount != null)
            throw new ConflictException("An account with this email already exists.");

        var roleSpecification = new GetRoleByShopIdOrAdminIdSpecification(Convert.ToInt32(staffCsvModel.Role), shopId);
        var role = await roleRepository.GetAsync(roleSpecification, cancelletionToken)
            ?? throw new NotFoundException("Role not found");

        var account = staffCsvModel.Adapt<AccountEntity>();
        account.ShopId = shopId;
        account.RoleId = role.Id;
        account.Status = StaffStatusEnum.CreatePassword;

        var initialPassword = Guid.NewGuid().ToString();
        account.PasswordHash = PasswordHasher.HashPassword(initialPassword);

        var createdAccount = await accountRepository.AddAsync(account, cancelletionToken);

        var emailModel = await emailBuilderService
            .BuildResetPasswordEmailAsync(staffCsvModel.Email, staffCsvModel.FirstName);

        await emailSender.SendAsync(emailModel, account.Id);
    }

    public async Task UpdateStaffAsync(
        UpdateStaffRequest request,
        AccountEntity staff,
        CancellationToken cancellationToken)
    {
        CheckIfDeactivated(request, staff);

        await UpdateEmailAsync(request, staff, cancellationToken);

        await UpdateActiveStatusAsync(request, staff, cancellationToken);

        request.Adapt(staff);
    }

    private async Task UpdateActiveStatusAsync(
        UpdateStaffRequest request,
        AccountEntity staff,
        CancellationToken cancellationToken)
    {
        if (!request.IsActive.HasValue)
        {
            return;
        }

        if (request.IsActive.Value && staff.Status == StaffStatusEnum.Deactivated)
        {
            staff.Status = staff.PasswordUpdatedAt == null
                ? StaffStatusEnum.CreatePassword
                : StaffStatusEnum.Offline;
        }
        else if (!request.IsActive.Value && staff.Status != StaffStatusEnum.Deactivated)
        {
            staff.Status = StaffStatusEnum.Deactivated;
            await sessionTerminationService.TerminateSessionsAsync(staff.Id, cancellationToken);
        }
    }

    private async Task UpdateEmailAsync(
        UpdateStaffRequest request,
        AccountEntity staff,
        CancellationToken cancellationToken)
    {
        if (request.Email == null || staff.Email == request.Email || staff.PasswordUpdatedAt == null)
            return;

        var emailConflictSpec = new GetAccountByEmailSpecification(request.Email);
        var existingWithEmail = await accountRepository.Exists(emailConflictSpec, cancellationToken);
        if (existingWithEmail)
            throw new ConflictException("Another account with this email already exists.");

        await sessionTerminationService.TerminateSessionsAsync(staff.Id, cancellationToken);

        staff.Status = StaffStatusEnum.Offline;
    }

    private void CheckIfDeactivated(
        UpdateStaffRequest request,
        AccountEntity staff)
    {
        if (staff.Status == StaffStatusEnum.Deactivated &&
                (!request.IsActive.HasValue || request.IsActive.Value == false))
        {
            throw new BadRequestException("You cannot edit a deactivated staff member unless you are reactivating them.");
        }
    }
}
