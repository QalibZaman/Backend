using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services.IStaffService;

public class StaffService(
    IStaffRepository accountRepository,
    IRoleRepository roleRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService,
    ISessionTerminationService sessionTerminationService,
    IMapper mapper,
    ICsvFileService csvFileService,
    ILogger<StaffService> logger) : IStaffService
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

        await accountRepository.AddAsync(account, cancelletionToken);

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

    public async Task<byte[]> DownloadStaffCsvAsync(
        int userShopId,
        GetFilteredStaffCsvRequest request,
        CancellationToken token = default)
    {
        var staffStatus = ParseStaffStatus(request.Status);

        var spec = new GetFilteredStaffSpecification(
            shopId: userShopId,
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            roleId: request.RoleId,
            status: staffStatus);

        List<StaffEntity> staff;

        try
        {
            staff = (await accountRepository.GetStaffAccounts(spec, token)).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve staff data for CSV export. ShopId: {ShopId}", userShopId);
            throw new BadRequestException(StaffDownloadErrors.PartialDataUnavailable);
        }

        if (staff.Count == 0)
            throw new BadRequestException(StaffDownloadErrors.EmptyFilteredList);

        if (HasIncompleteStaffData(staff))
        {
            logger.LogWarning("Incomplete staff data detected during CSV export. ShopId: {ShopId}", userShopId);
            throw new BadRequestException(StaffDownloadErrors.PartialDataUnavailable);
        }

        try
        {
            return await csvFileService.GetEntitiesCsvData<StaffCsvModel, StaffEntity>(staff, token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate staff CSV file. ShopId: {ShopId}", userShopId);
            throw new BadRequestException(StaffDownloadErrors.CsvGenerationFailed);
        }
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

    private static bool HasIncompleteStaffData(IEnumerable<StaffEntity> staff)
    {
        return staff.Any(x =>
            string.IsNullOrWhiteSpace(x.FirstName) ||
            string.IsNullOrWhiteSpace(x.LastName) ||
            string.IsNullOrWhiteSpace(x.Email) ||
            x.Role == null ||
            string.IsNullOrWhiteSpace(x.Role.Name));
    }
}
