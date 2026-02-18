using Allseating.Domain;

namespace Allseating.Application;

public class ListGames
{
    private readonly IGameRepository _repository;

    public ListGames(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListGamesResult> ExecuteAsync(ListGamesRequest request, CancellationToken ct = default)
    {
        var (items, totalCount) = await _repository.ListAsync(
            request.Offset,
            request.Limit,
            request.SortProp,
            request.SortDir,
            request.Q,
            request.Platform,
            request.Status,
            ct);

        var listItems = items.Select(g => new GameListItem
        {
            Id = g.Id,
            Barcode = g.Barcode,
            Title = g.Title,
            Description = g.Description,
            Platform = g.Platform,
            ReleaseDate = g.ReleaseDate,
            Status = g.Status,
            Price = g.Price
        }).ToList();

        return new ListGamesResult { Items = listItems, TotalCount = totalCount };
    }
}
