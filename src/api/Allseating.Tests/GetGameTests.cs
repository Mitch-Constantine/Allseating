using Allseating.Application;
using Allseating.Domain;
using Allseating.Tests.Fakes;
using Xunit;

namespace Allseating.Tests;

public class GetGameTests
{
    [Fact]
    public async Task ExecuteAsync_Found_ReturnsGameDetail()
    {
        var repo = new FakeGameRepository();
        var id = Guid.NewGuid();
        var game = new Game
        {
            Id = id,
            Barcode = "BAR",
            Title = "Title",
            Description = "Desc",
            Platform = Platform.SWITCH,
            ReleaseDate = new DateOnly(2025, 6, 1),
            Status = Status.Upcoming,
            Price = 59.99m,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            RowVersion = [1, 2, 3]
        };
        repo.Add(game);
        var useCase = new GetGame(repo);

        var result = await useCase.ExecuteAsync(id);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Title", result.Title);
        Assert.Equal(59.99m, result.Price);
        Assert.Equal(3, result.RowVersion.Length);
    }
}
