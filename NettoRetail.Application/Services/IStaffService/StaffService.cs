using MapsterMapper;
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
    IStaffRepository accountRepository,
    IRoleRepository roleRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService,
    ISessionTerminationService sessionTerminationService,
    IMapper mapper) : IStaffService
{
    public async Task CreateStaffAsync(StaffCsvModel staffCsvModel, int shopId, CancellationToken cancelletionToken = default)
    {
        var accountSpecification = new GetStaffByEmailSpecification(staffCsvModel.Email);
        var existingAccount = await accountRepository
            .GetAsync(accountSpecification, cancelletionToken);

        if (existingAccount != null)
            throw new ConflictException("An account with this email already exists.");

        var roleSpecification = new GetRoleByShopIdOrAdminIdSpecification(Convert.ToInt32(staffCsvModel.Role), shopId);
        var role = await roleRepository.GetAsync(roleSpecification, cancelletionToken)
            ?? throw new NotFoundException("Role not found");

        var account = mapper.Map<StaffEntity>(staffCsvModel);
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
        StaffEntity staff,
        CancellationToken cancellationToken)
    {
        CheckIfDeactivated(request, staff);

        await UpdateEmailAsync(request, staff, cancellationToken);

        await UpdateActiveStatusAsync(request, staff, cancellationToken);

        mapper.Map(request, staff);
    }

    public StaffStatusEnum? ParseStaffStatus(string? statusString)
    {
        if (string.IsNullOrWhiteSpace(statusString))
            return null;

        if (Enum.TryParse<StaffStatusEnum>(statusString.Replace(" ", ""), true, out var status))
        {
            return status;
        }

        return null;
    }

    private async Task UpdateActiveStatusAsync(
        UpdateStaffRequest request,
        StaffEntity staff,
        CancellationToken cancellationToken)
    {
        if (!request.IsActive.HasValue)
        {
            return;
        }

        if (request.IsActive.Value)
        {
            staff.Status = StaffStatusEnum.Active;
        }
        else if (!request.IsActive.Value && staff.Status != StaffStatusEnum.Deactivated)
        {
            staff.Status = StaffStatusEnum.Deactivated;
            await sessionTerminationService.TerminateSessionsAsync(staff.Id, cancellationToken);
        }
    }

    private async Task UpdateEmailAsync(
        UpdateStaffRequest request,
        StaffEntity staff,
        CancellationToken cancellationToken)
    {
        if (request.Email == null || staff.Email == request.Email || staff.PasswordUpdatedAt == null)
            return;

        var emailConflictSpec = new GetStaffByEmailSpecification(request.Email);
        var existingWithEmail = await accountRepository.Exists(emailConflictSpec, cancellationToken);
        if (existingWithEmail)
            throw new ConflictException("Another account with this email already exists.");

        await sessionTerminationService.TerminateSessionsAsync(staff.Id, cancellationToken);
    }

    private void CheckIfDeactivated(
        UpdateStaffRequest request,
        StaffEntity staff)
    {
        if (staff.Status == StaffStatusEnum.Deactivated &&
                (!request.IsActive.HasValue || request.IsActive.Value == false))
        {
            throw new BadRequestException("You cannot edit a deactivated staff member unless you are reactivating them.");
        }
    }
}
