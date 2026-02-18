using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Allseating.Tests;

/// <summary>
/// Uses SQL Server Express and a dedicated test database so tests don't affect dev data.
/// Requires SQL Server Express (e.g. .\SQLEXPRESS) to be available.
/// </summary>
public class AllseatingApiFactory : WebApplicationFactory<Program>
{
    private const string SqlExpressConnectionString =
        "Server=.\\SQLEXPRESS;Database=AllseatingDbTest;Trusted_Connection=True;TrustServerCertificate=True;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = SqlExpressConnectionString
            });
        });
    }
}
