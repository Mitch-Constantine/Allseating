using Allseating.Application;
using Allseating.Domain;

namespace Allseating.Tests.Fakes;

public sealed class FakeGameRepository : IGameRepository
{
    private readonly List<Game> _games = [];

    private readonly List<(int Offset, int Limit, string? SortProp, string? SortDir, string? SearchTerm, Platform? Platform, Status? Status)> _listAsyncCalls = [];
    public IReadOnlyList<(int Offset, int Limit, string? SortProp, string? SortDir, string? SearchTerm, Platform? Platform, Status? Status)> ListAsyncCalls => _listAsyncCalls;

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var g = _games.FirstOrDefault(x => x.Id == id);
        return Task.FromResult<Game?>(g);
    }

    public Task<Game?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        var g = _games.FirstOrDefault(x => x.Id == id);
        return Task.FromResult<Game?>(g);
    }

    public Task<(IReadOnlyList<Game> Items, int TotalCount)> ListAsync(int offset, int limit, string? sortProp, string? sortDir, string? searchTerm, Platform? platform, Status? status, CancellationToken ct = default)
    {
        _listAsyncCalls.Add((offset, limit, sortProp, sortDir, searchTerm, platform, status));

        var query = _games.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(g =>
                (g.Title?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (g.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (g.Barcode?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        if (platform.HasValue)
            query = query.Where(g => g.Platform == platform.Value);
        if (status.HasValue)
            query = query.Where(g => g.Status == status.Value);

        var list = query.ToList();
        var totalCount = list.Count;

        if (!string.IsNullOrEmpty(sortProp))
        {
            list = sortProp.ToLowerInvariant() switch
            {
                "title" => list.OrderBy(g => g.Title).ToList(),
                "barcode" => list.OrderBy(g => g.Barcode).ToList(),
                "releasedate" => list.OrderBy(g => g.ReleaseDate).ToList(),
                "price" => list.OrderBy(g => g.Price).ToList(),
                _ => list
            };
            if (string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase))
                list.Reverse();
        }

        var items = list.Skip(offset).Take(limit > 0 ? limit : int.MaxValue).ToList();
        return Task.FromResult<(IReadOnlyList<Game> Items, int TotalCount)>((items, totalCount));
    }

    public Task<bool> ExistsBarcodeAsync(string barcode, Guid? excludeId = null, CancellationToken ct = default)
    {
        var exists = _games.Any(g => string.Equals(g.Barcode, barcode.Trim(), StringComparison.OrdinalIgnoreCase) && g.Id != excludeId);
        return Task.FromResult(exists);
    }

    public Task<int> CountAsync(CancellationToken ct = default) => Task.FromResult(_games.Count);

    public void Add(Game game) => _games.Add(game);

    public void Remove(Game game) => _games.Remove(game);

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        if (_saveChangesException != null)
            throw _saveChangesException;
        return Task.CompletedTask;
    }

    public void SetSaveChangesThrowsDuplicateBarcode()
    {
        _saveChangesException = new DuplicateBarcodeException();
    }

    public void SetSaveChangesThrowsConcurrency()
    {
        _saveChangesException = new ConcurrencyException("Concurrency conflict.");
    }

    private Exception? _saveChangesException;
}
