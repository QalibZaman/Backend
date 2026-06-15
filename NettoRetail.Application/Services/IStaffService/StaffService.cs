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
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services.IStaffService;

public class StaffService(
    IStaffRepository accountRepository,
    IRoleRepository roleRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService,
    ISessionTerminationService sessionTerminationService,
    IMapper mapper,
    IUnitOfWork unitOfWork) : IStaffService
{
    public async Task CreateStaffAsync(
        StaffCsvModel staffCsvModel,
        int shopId,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(staffCsvModel.Role, out var roleId))
            throw new BadRequestException(string.Format(ValidationErrorMessages.StaffCsv.InvalidRoleIdFormatMessage, 0));

        await CreateSingleStaffCoreAsync(staffCsvModel, staffCsvModel.Email, roleId, shopId, cancellationToken);
    }

    public async Task<StaffEntity> CreateStaffAsync(
        CreateStaffRequest request,
        int shopId,
        CancellationToken cancellationToken = default)
    {
        return await CreateSingleStaffCoreAsync(request, request.Email, request.RoleId, shopId, cancellationToken);
    }

    public async Task CreateStaffAsync(
        IReadOnlyCollection<StaffCsvModel> staffCsvModels,
        int shopId,
        CancellationToken cancellationToken = default)
    {
        if (staffCsvModels == null || staffCsvModels.Count == 0)
            return;

        var rolesById = await ValidateCsvAndGetRolesAsync(staffCsvModels, shopId, cancellationToken);

        var accounts = staffCsvModels.Select(model =>
        {
            var account = mapper.Map<StaffEntity>(model);
            PrepareAccount(account, shopId, rolesById[int.Parse(model.Role)].Id);
            return account;
        }).ToList();

        await accountRepository.AddRangeAsync(accounts, cancellationToken);

        var emailTasks = accounts.Select(SendResetPasswordEmailAsync);
        await Task.WhenAll(emailTasks);
    }

    public async Task UpdateStaffAsync(
        UpdateStaffRequest request,
        StaffEntity staff,
        CancellationToken cancellationToken = default)
    {
        CheckIfDeactivated(request, staff);
        await UpdateEmailAsync(request, staff, cancellationToken);
        await UpdateActiveStatusAsync(request, staff, cancellationToken);

        mapper.Map(request, staff);
    }

    private static void PrepareAccount(StaffEntity account, int shopId, int roleId)
    {
        account.ShopId = shopId;
        account.RoleId = roleId;
        account.Status = StaffStatusEnum.CreatePassword;
        account.PasswordHash = PasswordHasher.HashPassword(Guid.NewGuid().ToString());
    }

    private async Task<StaffEntity> CreateSingleStaffCoreAsync<T>(
        T source,
        string email,
        int roleId,
        int shopId,
        CancellationToken cancellationToken)
    {
        var role = await GetRoleAsync(roleId, shopId, cancellationToken);
        await EnsureEmailAvailableAsync(email, cancellationToken);

        var account = mapper.Map<StaffEntity>(source);
        PrepareAccount(account, shopId, role.Id);

        using var transaction = unitOfWork.BeginTransaction();

        await accountRepository.AddAsync(account, cancellationToken);
        await SendResetPasswordEmailAsync(account);

        transaction.Commit();

        return account;
    }

    private async Task UpdateActiveStatusAsync(
        UpdateStaffRequest request,
        StaffEntity staff,
        CancellationToken cancellationToken)
    {
        if (!request.IsActive.HasValue)
            return;

        if (request.IsActive.Value)
        {
            staff.Status = StaffStatusEnum.Active;
        }
        else if (staff.Status != StaffStatusEnum.Deactivated)
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
        if (string.IsNullOrWhiteSpace(request.Email) || staff.Email == request.Email)
            return;

        var emailConflictSpec = new GetStaffByEmailSpecification(request.Email);
        var existingWithEmail = await accountRepository.ExistsAsync(emailConflictSpec, cancellationToken);
        if (existingWithEmail)
            throw new ConflictException(ValidationErrorMessages.Email.AlreadyExistsErrorMessage);

        await sessionTerminationService.TerminateSessionsAsync(staff.Id, cancellationToken);
    }

    private void CheckIfDeactivated(UpdateStaffRequest request, StaffEntity staff)
    {
        if (staff.Status == StaffStatusEnum.Deactivated && request.IsActive is not true)
        {
            throw new BadRequestException(ValidationErrorMessages.Staff.CannotEditDeactivatedStaffMemberErrorMessage);
        }
    }

    private async Task<Dictionary<int, RoleEntity>> ValidateCsvAndGetRolesAsync(
        IReadOnlyCollection<StaffCsvModel> staffCsvModels,
        int shopId,
        CancellationToken cancellationToken)
    {
        var validationErrors = new List<string>();
        var requestedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var requestedRoleIds = new HashSet<int>();
        var rowData = new List<(int Index, string Email, int RoleId)>();

        int index = CsvConstants.CsvDataStartRow;
        foreach (var staff in staffCsvModels)
        {
            var email = staff.Email?.Trim() ?? string.Empty;
            requestedEmails.Add(email);

            if (!int.TryParse(staff.Role, out var roleId))
            {
                validationErrors.Add(string.Format(ValidationErrorMessages.StaffCsv.InvalidRoleIdFormatMessage, index));
            }
            else
            {
                requestedRoleIds.Add(roleId);
                rowData.Add((index, email, roleId));
            }

            index++;
        }

        var existingAccounts = await accountRepository.GetAllBySpecAsync(
            new GetStaffByEmailsSpecification(requestedEmails.ToList()), cancellationToken);

        var existingEmails = existingAccounts
            .Select(a => a.Email)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var roles = requestedRoleIds.Count > 0
            ? await roleRepository.GetAllBySpecAsync(new GetRolesByIdsAndShopIdSpecification(requestedRoleIds.ToList(), shopId), cancellationToken)
            : new List<RoleEntity>();

        var rolesById = roles.ToDictionary(role => role.Id);

        foreach (var row in rowData)
        {
            if (existingEmails.Contains(row.Email))
                validationErrors.Add(string.Format(ValidationErrorMessages.StaffCsv.AccountWithEmailAlreadyExistsMessage, row.Index, row.Email));

            if (!rolesById.ContainsKey(row.RoleId))
                validationErrors.Add(string.Format(ValidationErrorMessages.StaffCsv.RoleWithIdNotFoundMessage, row.Index, row.RoleId));
        }

        if (validationErrors.Count > 0)
            throw new BadRequestException($"{CsvUploadErrors.StaffListValidationFailed}\n{string.Join("\n", validationErrors)}");

        return rolesById;
    }

    private async Task EnsureEmailAvailableAsync(string email, CancellationToken cancellationToken)
    {
        var emailConflictSpec = new GetStaffByEmailSpecification(email);
        var exists = await accountRepository.ExistsAsync(emailConflictSpec, cancellationToken);

        if (exists)
            throw new ConflictException(ValidationErrorMessages.Email.AlreadyExistsErrorMessage);
    }

    private async Task<RoleEntity> GetRoleAsync(int roleId, int shopId, CancellationToken cancellationToken)
    {
        var roleSpecification = new GetRoleByShopIdOrAdminIdSpecification(roleId, shopId);
        return await roleRepository.GetAsync(roleSpecification, cancellationToken)
            ?? throw new NotFoundException(ValidationErrorMessages.Role.RoleNotFoundErrorMessage);
    }

    private async Task SendResetPasswordEmailAsync(StaffEntity account)
    {
        var emailModel = await emailBuilderService.BuildResetPasswordEmailAsync(account.Email, account.FirstName);
        await emailSender.SendAsync(emailModel, account.Id);
    }
}