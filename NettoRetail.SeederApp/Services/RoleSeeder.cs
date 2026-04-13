using NettoRetail.Domain.Enums;
using Microsoft.Extensions.Configuration;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.SeederApp.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.SeederApp.Services;

public class RoleSeeder(IConfiguration config, IStaffRepository repository)
{
    public async Task SeedAsync()
    {
        var accounts = config
            .GetSection("Accounts")
            .Get<IEnumerable<AccountSeedModel>>();

        if (accounts is null || !accounts.Any())
        {
            Console.WriteLine("No accounts found in configuration.");
            return;
        }

        foreach (var acc in accounts)
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

            var account = new StaffEntity
            {
                FirstName = acc.FirstName,
                LastName = acc.LastName,
                Email = acc.Email,
                RoleId = acc.RoleId,
                Status = status,
                PasswordHash = PasswordHasher.HashPassword(acc.Password)
            };

            await repository.AddAsync(account, CancellationToken.None);
            Console.WriteLine($"Created account: {acc.Email}");
        }
    }
}
