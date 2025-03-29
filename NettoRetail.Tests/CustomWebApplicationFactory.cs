using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.DataAccess.Context;
using Npgsql;

namespace NettoRetail.Tests;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var adminConnection = new NpgsqlConnection("Server=localhost;Port=5432;User Id=postgres;Password=admin;");
            adminConnection.Open();

            try
            {
                var cmd = new NpgsqlCommand(
                    "SELECT 1 FROM pg_database WHERE datname = 'nrt_test'",
                    adminConnection);
                var exists = cmd.ExecuteScalar() != null;

                if (!exists)
                {
                    cmd.CommandText = "CREATE DATABASE nrt_test";
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                adminConnection.Close();
            }

            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new NpgsqlConnection("Server=localhost;Database=nrt_test;Port=5432;User Id=postgres;Password=admin;");
                connection.Open();
                return connection;
            });

            services.AddDbContext<AppDbContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseNpgsql((NpgsqlConnection)connection);
            });
        });
    }
}