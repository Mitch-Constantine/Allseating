using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Allseating.Api.Dto;
using Xunit;

namespace Allseating.Tests;

public class ConcurrencyIntegrationTests : IClassFixture<AllseatingApiFactory>
{
    private readonly HttpClient _client;

    public ConcurrencyIntegrationTests(AllseatingApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Update_WithStaleRowVersion_Returns409ConcurrencyConflict()
    {
        var barcode = "CONC-TEST-" + Guid.NewGuid().ToString("N")[..8];
        var createDto = new CreateGameDto(
            barcode,
            "Concurrency Game",
            "Description",
            "PC",
            new DateOnly(2026, 12, 1),
            "Upcoming",
            29.99m);

        var createRes = await _client.PostAsJsonAsync("/api/games", createDto);
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<GameDetailDto>();
        Assert.NotNull(created);

        var staleRowVersion = (byte[])created.RowVersion.Clone();

        var updateDto = new UpdateGameDto(
            barcode,
            "Updated Once",
            "Desc",
            "PC",
            new DateOnly(2026, 12, 1),
            "Upcoming",
            39.99m,
            created.RowVersion);

        var firstUpdate = await _client.PutAsJsonAsync($"/api/games/{created.Id}", updateDto);
        firstUpdate.EnsureSuccessStatusCode();

        var staleUpdateDto = new UpdateGameDto(
            barcode,
            "Stale Update",
            "Desc",
            "PC",
            new DateOnly(2026, 12, 1),
            "Upcoming",
            49.99m,
            staleRowVersion);

        var staleUpdate = await _client.PutAsJsonAsync($"/api/games/{created.Id}", staleUpdateDto);
        Assert.Equal(HttpStatusCode.Conflict, staleUpdate.StatusCode);

        var json = await staleUpdate.Content.ReadAsStringAsync();
        Assert.Contains("ConcurrencyConflict", json);
        var problem = await staleUpdate.Content.ReadFromJsonAsync<ProblemDetailsWithErrorCode>();
        Assert.NotNull(problem);
        Assert.Equal("ConcurrencyConflict", problem.ErrorCode);
    }

    private sealed class ProblemDetailsWithErrorCode
    {
        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }
    }
}
