using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
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
        var role = await GetRoleAsync(int.Parse(staffCsvModel.Role), shopId, cancelletionToken);
        await EnsureEmailAvailableAsync(staffCsvModel.Email, cancelletionToken);

        var account = mapper.Map<StaffEntity>(staffCsvModel);
        PrepareAccount(account, shopId, role.Id);

        await accountRepository.AddAsync(account, cancelletionToken);
        await SendResetPasswordEmailAsync(account);
    }

    public async Task<StaffEntity> CreateStaffAsync(
        CreateStaffRequest request,
        int shopId,
        CancellationToken token = default)
    {
        var role = await GetRoleAsync(request.RoleId, shopId, token);
        await EnsureEmailAvailableAsync(request.Email, token);

        var account = mapper.Map<StaffEntity>(request);
        PrepareAccount(account, shopId, role.Id);

        await accountRepository.AddAsync(account, token);
        await SendResetPasswordEmailAsync(account);

        return account;
    }

    public async Task CreateStaffAsync(
        IReadOnlyCollection<StaffCsvModel> staffCsvModels,
        int shopId,
        CancellationToken token = default)
    {
        if (staffCsvModels.Count == 0)
            return;

        var rolesById = await GetImportRolesByIdAsync(staffCsvModels, shopId, token);

        var accounts = new List<StaffEntity>(staffCsvModels.Count);
        foreach (var staffCsvModel in staffCsvModels)
        {
            var account = mapper.Map<StaffEntity>(staffCsvModel);
            PrepareAccount(account, shopId, rolesById[int.Parse(staffCsvModel.Role)].Id);
            accounts.Add(account);
        }

        await accountRepository.AddRangeAsync(accounts, token);

        foreach (var account in accounts)
        {
            await SendResetPasswordEmailAsync(account);
        }
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
        if (request.Email == null || staff.Email == request.Email)
            return;

        var emailConflictSpec = new GetStaffByEmailSpecification(request.Email);
        var existingWithEmail = await accountRepository.ExistsAsync(emailConflictSpec, cancellationToken);
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

    private async Task<Dictionary<int, RoleEntity>> GetImportRolesByIdAsync(
        IReadOnlyCollection<StaffCsvModel> staffCsvModels,
        int shopId,
        CancellationToken cancellationToken)
    {
        var validationErrors = new List<string>();
        var requestedEmails = staffCsvModels.Select(staff => staff.Email.Trim()).ToList();
        var existingAccounts = await accountRepository.GetAllBySpecAsync(
            new GetStaffByEmailsSpecification(requestedEmails),
            cancellationToken);
        var existingEmails = existingAccounts
            .Select(account => account.Email)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (staff, index) in staffCsvModels.Select((staff, index) => (staff, index + 2)))
        {
            if (existingEmails.Contains(staff.Email))
                validationErrors.Add($"Row {index}: Account with email '{staff.Email}' already exists.");
        }

        var requestedRoleIds = staffCsvModels.Select(staff => int.Parse(staff.Role)).Distinct().ToList();
        var roles = await roleRepository.GetAllBySpecAsync(
            new GetRolesByIdsAndShopIdSpecification(requestedRoleIds, shopId),
            cancellationToken);
        var existingRoleIds = roles.Select(role => role.Id).ToHashSet();

        foreach (var (staff, index) in staffCsvModels.Select((staff, index) => (staff, index + 2)))
        {
            var roleId = int.Parse(staff.Role);
            if (!existingRoleIds.Contains(roleId))
                validationErrors.Add($"Row {index}: Role with id '{roleId}' was not found.");
        }

        if (validationErrors.Count > 0)
            throw new BadRequestException($"{CsvUploadErrors.StaffListValidationFailed}\n{string.Join("\n", validationErrors)}");

        return roles.ToDictionary(role => role.Id);
    }

    private async Task EnsureEmailAvailableAsync(string email, CancellationToken cancellationToken)
    {
        var accountSpecification = new GetStaffByEmailSpecification(email);
        var existingAccount = await accountRepository.GetAsync(accountSpecification, cancellationToken);

        if (existingAccount != null)
            throw new ConflictException("An account with this email already exists.");
    }

    private async Task<RoleEntity> GetRoleAsync(int roleId, int shopId, CancellationToken cancellationToken)
    {
        var roleSpecification = new GetRoleByShopIdOrAdminIdSpecification(roleId, shopId);
        return await roleRepository.GetAsync(roleSpecification, cancellationToken)
            ?? throw new NotFoundException("Role not found");
    }

    private static void PrepareAccount(StaffEntity account, int shopId, int roleId)
    {
        account.ShopId = shopId;
        account.RoleId = roleId;
        account.Status = StaffStatusEnum.CreatePassword;
        account.PasswordHash = PasswordHasher.HashPassword(Guid.NewGuid().ToString());
    }

    private async Task SendResetPasswordEmailAsync(StaffEntity account)
    {
        var emailModel = await emailBuilderService
            .BuildResetPasswordEmailAsync(account.Email, account.FirstName);

        await emailSender.SendAsync(emailModel, account.Id);
    }
}
