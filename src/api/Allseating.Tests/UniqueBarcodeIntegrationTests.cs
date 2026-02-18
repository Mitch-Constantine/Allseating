using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Allseating.Api.Dto;
using Xunit;

namespace Allseating.Tests;

public class UniqueBarcodeIntegrationTests : IClassFixture<AllseatingApiFactory>
{
    private readonly HttpClient _client;

    public UniqueBarcodeIntegrationTests(AllseatingApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithDuplicateBarcode_ReturnsBadRequest()
    {
        var barcode = "INT-TEST-" + Guid.NewGuid().ToString("N")[..8];
        var dto = new CreateGameDto(
            barcode,
            "First Game",
            "Description",
            "PC",
            new DateOnly(2026, 12, 1),
            "Upcoming",
            49.99m);

        var create1 = await _client.PostAsJsonAsync("/api/games", dto);
        create1.EnsureSuccessStatusCode();

        var dto2 = dto with { Title = "Second Game" };
        var create2 = await _client.PostAsJsonAsync("/api/games", dto2);
        Assert.Equal(HttpStatusCode.BadRequest, create2.StatusCode);
    }
}
