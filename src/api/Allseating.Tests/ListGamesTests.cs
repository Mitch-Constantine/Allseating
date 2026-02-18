using Allseating.Application;
using Allseating.Domain;
using Allseating.Tests.Fakes;
using Xunit;

namespace Allseating.Tests;

public class ListGamesTests
{
    [Fact]
    public async Task ExecuteAsync_CallsRepositoryWithRequestParams()
    {
        var repo = new FakeGameRepository();
        var useCase = new ListGames(repo);
        var request = new ListGamesRequest
        {
            Offset = 20,
            Limit = 5,
            SortProp = "title",
            SortDir = "desc",
            Q = "search",
            Platform = Platform.PC,
            Status = Status.Active
        };
        await useCase.ExecuteAsync(request);
        Assert.Single(repo.ListAsyncCalls);
        var call = repo.ListAsyncCalls[0];
        Assert.Equal(20, call.Offset);
        Assert.Equal(5, call.Limit);
        Assert.Equal("title", call.SortProp);
        Assert.Equal("desc", call.SortDir);
        Assert.Equal("search", call.SearchTerm);
        Assert.Equal(Platform.PC, call.Platform);
        Assert.Equal(Status.Active, call.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMappedItemsAndTotalCount()
    {
        var repo = new FakeGameRepository();
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Barcode = "BAR",
            Title = "Title",
            Description = "Desc",
            Platform = Platform.PS5,
            ReleaseDate = new DateOnly(2025, 1, 1),
            Status = Status.Active,
            Price = 29.99m,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            RowVersion = [1]
        };
        repo.Add(game);
        var useCase = new ListGames(repo);
        var result = await useCase.ExecuteAsync(new ListGamesRequest { Limit = 10 });
        Assert.Equal(1, result.TotalCount);
        var item = Assert.Single(result.Items);
        Assert.Equal(game.Id, item.Id);
        Assert.Equal(game.Title, item.Title);
        Assert.Equal(game.Price, item.Price);
    }
}
