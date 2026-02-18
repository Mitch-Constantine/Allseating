using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Allseating.Application;
using Allseating.Domain;

namespace Allseating.Infrastructure;

public sealed class GameRepository : IGameRepository
{
    private readonly AppDbContext _db;

    public GameRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _db.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<Game?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default) =>
        await _db.Games.FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<(IReadOnlyList<Game> Items, int TotalCount)> ListAsync(int offset, int limit, string? sortProp, string? sortDir, string? searchTerm, Platform? platform, Status? status, CancellationToken ct = default)
    {
        var query = _db.Games.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(g =>
                g.Title.ToLower().Contains(term) ||
                g.Description.ToLower().Contains(term) ||
                g.Barcode.ToLower().Contains(term));
        }

        if (platform.HasValue)
            query = query.Where(g => g.Platform == platform.Value);
        if (status.HasValue)
            query = query.Where(g => g.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var dir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = ApplySort(query, sortProp, dir);

        var items = await query.Skip(offset).Take(limit).ToListAsync(ct);
        return (items, totalCount);
    }

    private static IQueryable<Game> ApplySort(IQueryable<Game> query, string? sortProp, bool descending)
    {
        return sortProp?.ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(g => g.Title) : query.OrderBy(g => g.Title),
            "barcode" => descending ? query.OrderByDescending(g => g.Barcode) : query.OrderBy(g => g.Barcode),
            "platform" => descending ? query.OrderByDescending(g => g.Platform) : query.OrderBy(g => g.Platform),
            "status" => descending ? query.OrderByDescending(g => g.Status) : query.OrderBy(g => g.Status),
            "releasedate" => descending ? query.OrderByDescending(g => g.ReleaseDate) : query.OrderBy(g => g.ReleaseDate),
            "price" => descending ? query.OrderByDescending(g => g.Price) : query.OrderBy(g => g.Price),
            _ => query.OrderBy(g => g.Title)
        };
    }

    public async Task<bool> ExistsBarcodeAsync(string barcode, Guid? excludeId = null, CancellationToken ct = default)
    {
        var q = _db.Games.Where(g => g.Barcode == barcode);
        if (excludeId.HasValue)
            q = q.Where(g => g.Id != excludeId.Value);
        return await q.AnyAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default) =>
        await _db.Games.CountAsync(ct);

    public void Add(Game game) => _db.Games.Add(game);

    public void Remove(Game game) => _db.Games.Remove(game);

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException(ex.Message);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateBarcodeException();
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is not SqlException sqlEx) return false;
        return sqlEx.Number is 2627 or 2601; // Unique constraint / duplicate key
    }
}
