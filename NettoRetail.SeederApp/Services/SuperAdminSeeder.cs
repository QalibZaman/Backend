using Mapster;
using Microsoft.Extensions.Configuration;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.SeederApp.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.SeederApp.Services;

public class SuperAdminSeeder(IConfiguration config, IAccountRepository repository)
{
    public async Task SeedAsync()
    {
        var superAdmins = config
            .GetSection("SuperAdmins")
            .Get<IEnumerable<SuperAdminSeedModel>>();

        var roleId = config.GetValue<int>("RoleId");

        if (superAdmins == null)
        {
            Console.WriteLine("No SuperAdmins found in configuration.");
            return;
        }

        foreach (var admin in superAdmins)
        {
            var specification = new GetAccountByEmailSpecification(admin.Email);
            var existingAccount = await repository.GetAsync(specification, CancellationToken.None);

            if (existingAccount != null)
            {
                Console.WriteLine("account already exists.");
                continue;
            }

            var account = admin.Adapt<AccountEntity>();

            account.PasswordHash = PasswordHasher.HashPassword(admin.Password);
            account.RoleId = roleId;

            await repository.AddAsync(account, CancellationToken.None);

            Console.WriteLine($"Created SuperAdmin: {admin.Email}");
        }
    }
}
