using Mapster;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.Services.IStaffService;

public class StaffService : IStaffServiсe
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailBuilderService _emailBuilderService;

    public StaffService(
        IAccountRepository accountRepository,
        IRoleRepository roleRepository,
        IEmailSender emailSender,
        IEmailBuilderService emailBuilderService)
    {
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
        _emailSender = emailSender;
        _emailBuilderService = emailBuilderService;
    }

    public async Task CreateStaffAsync(StaffCsvModel staffCsvModel, int shopId, CancellationToken cancelletionToken = default)
    {
        var accountSpecification = new GetAccountByEmailSpecification(staffCsvModel.Email);
        var existingAccount = await _accountRepository
            .GetAsync(accountSpecification, cancelletionToken);

        if (existingAccount != null)
            throw new ConflictException("An account with this email already exists.");

        var roleSpecification = new GetRoleByShopIdOrAdminIdSpecification(Convert.ToInt32(staffCsvModel.Role), shopId);
        var role = await _roleRepository.GetAsync(roleSpecification, cancelletionToken)
            ?? throw new NotFoundException("Role not found");

        var account = staffCsvModel.Adapt<AccountEntity>();
        account.ShopId = shopId;
        account.RoleId = role.Id;
        account.Status = StaffStatusEnum.CreatePassword;

        var initialPassword = Guid.NewGuid().ToString();
        account.PasswordHash = PasswordHasher.HashPassword(initialPassword);

        var createdAccount = await _accountRepository.AddAsync(account, cancelletionToken);

        var emailModel = await _emailBuilderService
            .BuildResetPasswordEmailAsync(staffCsvModel.Email, staffCsvModel.FirstName);

        await _emailSender.SendAsync(emailModel);
    }
}
