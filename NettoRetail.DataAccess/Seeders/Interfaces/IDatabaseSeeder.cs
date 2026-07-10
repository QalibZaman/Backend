namespace NettoRetail.DataAccess.Seeders.Interfaces;

public interface IDatabaseSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}