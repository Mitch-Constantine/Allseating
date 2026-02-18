using Allseating.Domain;

namespace Allseating.Application;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Returns a tracked entity for update/delete.</summary>
    Task<Game?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Game> Items, int TotalCount)> ListAsync(int offset, int limit, string? sortProp, string? sortDir, string? searchTerm, Platform? platform, Status? status, CancellationToken ct = default);
    Task<bool> ExistsBarcodeAsync(string barcode, Guid? excludeId = null, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    void Add(Game game);
    void Remove(Game game);
    Task SaveChangesAsync(CancellationToken ct = default);
}
