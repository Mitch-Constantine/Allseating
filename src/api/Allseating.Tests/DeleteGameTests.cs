using Allseating.Application;
using Allseating.Domain;
using Allseating.Tests.Fakes;
using Xunit;

namespace Allseating.Tests;

public class DeleteGameTests
{
    private readonly FakeGameRepository _repo = new();

    [Fact]
    public async Task ExecuteAsync_NotFound_ReturnsNotFound()
    {
        var useCase = new DeleteGame(_repo);
        var result = await useCase.ExecuteAsync(Guid.NewGuid());
        Assert.IsType<ApplicationResult<Unit>.NotFound>(result);
    }

    [Fact]
    public async Task ExecuteAsync_StatusNotDiscontinued_ReturnsConflictInvalidState()
    {
        var id = Guid.NewGuid();
        _repo.Add(new Game
        {
            Id = id,
            Barcode = "DEL-123",
            Title = "Active Game",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = new DateOnly(2025, 1, 1),
            Status = Status.Active,
            Price = 1,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            RowVersion = [1]
        });
        var useCase = new DeleteGame(_repo);
        var result = await useCase.ExecuteAsync(id);
        var conflict = Assert.IsType<ApplicationResult<Unit>.Conflict>(result);
        Assert.Equal(ConflictCode.InvalidState, conflict.Code);
    }

    [Fact]
    public async Task ExecuteAsync_Discontinued_ReturnsOkAndRemovesGame()
    {
        var id = Guid.NewGuid();
        var game = new Game
        {
            Id = id,
            Barcode = "DISC-123",
            Title = "Discontinued",
            Description = "D",
            Platform = Platform.PC,
            ReleaseDate = null,
            Status = Status.Discontinued,
            Price = 1,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            RowVersion = [1]
        };
        _repo.Add(game);
        var useCase = new DeleteGame(_repo);
        var result = await useCase.ExecuteAsync(id);
        Assert.IsType<ApplicationResult<Unit>.Ok>(result);
        Assert.Equal(0, await _repo.CountAsync());
    }
}
