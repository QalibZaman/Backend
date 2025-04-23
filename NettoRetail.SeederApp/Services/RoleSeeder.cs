using Mapster;
using Microsoft.Extensions.Configuration;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.SeederApp.Models;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.SeederApp.Services;

public class RoleSeeder(IConfiguration config, IAccountRepository repository)
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
                new GetAccountByEmailSpecification(acc.Email),
                CancellationToken.None);

            if (exists != null)
            {
                Console.WriteLine($"Account already exists: {acc.Email}");
                continue;
            }

            var account = acc.Adapt<AccountEntity>();
            account.PasswordHash = PasswordHasher.HashPassword(acc.Password);

            await repository.AddAsync(account, CancellationToken.None);
            Console.WriteLine($"Created account: {acc.Email}");
        }
    }
}

