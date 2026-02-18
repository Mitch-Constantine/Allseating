using Allseating.Application;
using Allseating.Domain;
using Allseating.Tests.Fakes;
using Xunit;

namespace Allseating.Tests;

public class CreateGameTests
{
    private readonly FakeGameRepository _repo = new();
    private readonly FakeClock _clock = new();

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task ExecuteAsync_InvalidPrice_ReturnsValidationError(decimal price)
    {
        _clock.UtcNow = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var useCase = new CreateGame(_repo, _clock);
        var request = new CreateGameRequest
        {
            Barcode = "1234567890123",
            Title = "Game",
            Description = "Desc",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2026, 1, 1),
            Status = Status.Upcoming,
            Price = price
        };

        var result = await useCase.ExecuteAsync(request);

        var err = Assert.IsType<ApplicationResult<GameDetail>.ValidationErrors>(result);
        Assert.Contains(err.Errors, e => e.Contains("Price", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ReturnsOkAndStoresGame()
    {
        _clock.UtcNow = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var useCase = new CreateGame(_repo, _clock);
        var request = new CreateGameRequest
        {
            Barcode = "1234567890123",
            Title = "Valid Game",
            Description = "Description",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2026, 1, 1),
            Status = Status.Upcoming,
            Price = 49.99m
        };

        var result = await useCase.ExecuteAsync(request);

        var ok = Assert.IsType<ApplicationResult<GameDetail>.Ok>(result);
        Assert.Equal(request.Title, ok.Value.Title);
        Assert.Equal(1, await _repo.CountAsync());
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateBarcodeFromExists_ReturnsValidationError()
    {
        _repo.Add(new Game
        {
            Id = Guid.NewGuid(),
            Barcode = "DUP-1234567890",
            Title = "Existing",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = null,
            Status = Status.Discontinued,
            Price = 1,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            RowVersion = [1, 2]
        });
        var useCase = new CreateGame(_repo, _clock);
        var request = new CreateGameRequest
        {
            Barcode = "DUP-1234567890",
            Title = "New",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2026, 1, 1),
            Status = Status.Upcoming,
            Price = 10
        };

        var result = await useCase.ExecuteAsync(request);

        var err = Assert.IsType<ApplicationResult<GameDetail>.ValidationErrors>(result);
        Assert.Single(err.Errors);
        Assert.Contains("Barcode", err.Errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_SaveChangesThrowsDuplicateBarcode_ReturnsValidationError()
    {
        _clock.UtcNow = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        _repo.SetSaveChangesThrowsDuplicateBarcode();
        var useCase = new CreateGame(_repo, _clock);
        var request = new CreateGameRequest
        {
            Barcode = "RACE-123456789",
            Title = "Game",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2026, 1, 1),
            Status = Status.Upcoming,
            Price = 10
        };

        var result = await useCase.ExecuteAsync(request);

        var err = Assert.IsType<ApplicationResult<GameDetail>.ValidationErrors>(result);
        Assert.Contains(err.Errors, e => e.Contains("Barcode", StringComparison.OrdinalIgnoreCase));
    }
}
