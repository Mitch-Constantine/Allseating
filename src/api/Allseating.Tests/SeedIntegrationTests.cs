using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Allseating.Api.Dto;
using Xunit;

namespace Allseating.Tests;

/// <summary>
/// Tests seeding behaviour: empty DB gets seeded; non-empty DB does not re-seed.
/// Uses a dedicated database (AllseatingDbSeedTest) so the first host start sees an empty DB.
/// </summary>
public class SeedIntegrationTests
{
    private const string SeedTestConnectionString =
        "Server=.\\SQLEXPRESS;Database=AllseatingDbSeedTest;Trusted_Connection=True;TrustServerCertificate=True;";

    [Fact]
    public async Task EmptyDb_ThenStartApp_SeedsFromFile()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(Environments.Development);
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = SeedTestConnectionString
                    });
                });
            });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/games?limit=1&offset=0");
        response.EnsureSuccessStatusCode();
        var list = await response.Content.ReadFromJsonAsync<GamesListResponse>();
        Assert.NotNull(list);
        Assert.True(list.TotalCount >= 1, "Expected at least one game from seed when DB was empty at startup.");
    }

    [Fact]
    public async Task NonEmptyDb_SecondAppStart_DoesNotReSeed()
    {
        // Use a unique DB for this test so no other test or run can add games and break the assertion.
        var uniqueDbName = "AllseatingDbSeedTest_NoReseed_" + Guid.NewGuid().ToString("N")[..8];
        var connectionString = $"Server=.\\SQLEXPRESS;Database={uniqueDbName};Trusted_Connection=True;TrustServerCertificate=True;";

        using var factory1 = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(Environments.Development);
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = connectionString
                    });
                });
            });
        var client1 = factory1.CreateClient();
        var countAfterFirstStart = await GetStableTotalCountAsync(client1);
        Assert.True(countAfterFirstStart >= 1, "Precondition: first start should have seeded at least one game.");

        using var factory2 = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(Environments.Development);
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = connectionString
                    });
                });
            });
        var client2 = factory2.CreateClient();
        var response2 = await client2.GetAsync("/api/games?limit=1&offset=0");
        response2.EnsureSuccessStatusCode();
        var list2 = await response2.Content.ReadFromJsonAsync<GamesListResponse>();
        Assert.NotNull(list2);
        Assert.Equal(countAfterFirstStart, list2.TotalCount);
    }

    private static async Task<int> GetStableTotalCountAsync(HttpClient client, int maxAttempts = 10)
    {
        var lastCount = -1;
        for (var i = 0; i < maxAttempts; i++)
        {
            var response = await client.GetAsync("/api/games?limit=1&offset=0");
            response.EnsureSuccessStatusCode();
            var list = await response.Content.ReadFromJsonAsync<GamesListResponse>();
            Assert.NotNull(list);
            if (list.TotalCount == lastCount)
                return list.TotalCount;
            lastCount = list.TotalCount;
            await Task.Delay(100);
        }
        return lastCount;
    }
}
