using Allseating.Application;
using Allseating.Domain;
using Allseating.Tests.Fakes;
using Xunit;

namespace Allseating.Tests;

public class UpdateGameTests
{
    private readonly FakeGameRepository _repo = new();
    private readonly FakeClock _clock = new();

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    public async Task ExecuteAsync_InvalidPrice_ReturnsValidationError(decimal price)
    {
        var id = Guid.NewGuid();
        var rowVersion = new byte[] { 1, 2 };
        _repo.Add(new Game
        {
            Id = id,
            Barcode = "BAR",
            Title = "T",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2025, 1, 1),
            Status = Status.Active,
            Price = 1,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            RowVersion = rowVersion
        });
        var useCase = new UpdateGame(_repo, _clock);
        var request = new UpdateGameRequest
        {
            Barcode = "BAR",
            Title = "T",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2025, 1, 1),
            Status = Status.Active,
            Price = price,
            RowVersion = rowVersion
        };

        var result = await useCase.ExecuteAsync(id, request);

        var err = Assert.IsType<ApplicationResult<GameDetail>.ValidationErrors>(result);
        Assert.Contains(err.Errors, e => e.Contains("Price", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_NotFound_ReturnsNotFound()
    {
        var useCase = new UpdateGame(_repo, _clock);
        var request = new UpdateGameRequest
        {
            Barcode = "BAR",
            Title = "T",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2025, 1, 1),
            Status = Status.Active,
            Price = 1,
            RowVersion = new byte[] { 1 }
        };

        var result = await useCase.ExecuteAsync(Guid.NewGuid(), request);

        Assert.IsType<ApplicationResult<GameDetail>.NotFound>(result);
    }

    [Fact]
    public async Task ExecuteAsync_StaleRowVersion_ReturnsConcurrencyConflict()
    {
        var id = Guid.NewGuid();
        var currentRowVersion = new byte[] { 1, 2 };
        _repo.Add(new Game
        {
            Id = id,
            Barcode = "BAR",
            Title = "T",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2025, 1, 1),
            Status = Status.Active,
            Price = 1,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            RowVersion = currentRowVersion
        });
        var useCase = new UpdateGame(_repo, _clock);
        var request = new UpdateGameRequest
        {
            Barcode = "BAR",
            Title = "T",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2025, 1, 1),
            Status = Status.Active,
            Price = 5,
            RowVersion = new byte[] { 9, 9 }
        };

        var result = await useCase.ExecuteAsync(id, request);

        var conflict = Assert.IsType<ApplicationResult<GameDetail>.Conflict>(result);
        Assert.Equal(ConflictCode.ConcurrencyConflict, conflict.Code);
    }

    [Fact]
    public async Task ExecuteAsync_Valid_ReturnsOkAndUpdatesGame()
    {
        _clock.UtcNow = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var id = Guid.NewGuid();
        var rowVersion = new byte[] { 1, 2 };
        _repo.Add(new Game
        {
            Id = id,
            Barcode = "BAR",
            Title = "Old",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2025, 1, 1),
            Status = Status.Active,
            Price = 1,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            RowVersion = rowVersion
        });
        var useCase = new UpdateGame(_repo, _clock);
        var request = new UpdateGameRequest
        {
            Barcode = "BAR",
            Title = "New Title",
            Description = "New Desc",
            Platform = Platform.PS5,
            ReleaseDate = new DateOnly(2025, 1, 1),
            Status = Status.Active,
            Price = 19.99m,
            RowVersion = rowVersion
        };

        var result = await useCase.ExecuteAsync(id, request);

        var ok = Assert.IsType<ApplicationResult<GameDetail>.Ok>(result);
        Assert.Equal("New Title", ok.Value.Title);
        Assert.Equal(19.99m, ok.Value.Price);
    }
}
