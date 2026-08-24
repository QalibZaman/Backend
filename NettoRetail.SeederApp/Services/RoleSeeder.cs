using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using NettoRetail.Domain.Enums;
using Microsoft.Extensions.Configuration;
using NettoRetail.DataAccess.Identity;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.SeederApp.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.SeederApp.Services;

public class RoleSeeder(IConfiguration config, IStaffRepository repository, UserManager<ApplicationUser> userManager)
{
    public async Task SeedAsync()
    {
        var accounts = config
            .GetSection("Accounts")
            .Get<IEnumerable<AccountSeedModel>>();

        var accountSeedModels = accounts as AccountSeedModel[] ?? accounts.ToArray();
        if (!accountSeedModels.Any())
        {
            Console.WriteLine("No accounts found in configuration.");
            return;
        }

        foreach (var acc in accountSeedModels)
        {
            var exists = await repository.GetAsync(
                new GetStaffByEmailSpecification(acc.Email),
                CancellationToken.None);

            if (exists != null)
            {
                Console.WriteLine($"Account already exists: {acc.Email}");
                continue;
            }

            var status = Enum.TryParse<StaffStatusEnum>(acc.Status, true, out var parsedStatus)
                ? parsedStatus
                : StaffStatusEnum.CreatePassword;
            var id = Guid.NewGuid();
            var account = new StaffEntity
            {
                Id = id,
                FirstName = acc.FirstName,
                LastName = acc.LastName,
                Email = acc.Email,
                RoleId = acc.RoleId,
                Status = status,
                PasswordHash = PasswordHasher.HashPassword(acc.Password),

            };

            var identityUser = new ApplicationUser
            {
                Id = id,
                Email = acc.Email,
                UserName = acc.Email
            };

            var result = await userManager.CreateAsync(identityUser, acc.Password);
            if (!result.Succeeded)
            {
                throw new ValidationException(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            await repository.AddAsync(account, CancellationToken.None);
            Console.WriteLine($"Created account: {acc.Email}");
        }
    }
}
