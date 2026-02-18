using Allseating.Domain;

namespace Allseating.Application;

public sealed class ListGamesRequest
{
    public int Offset { get; init; }
    public int Limit { get; init; } = 10;
    public string? SortProp { get; init; }
    public string? SortDir { get; init; }
    public string? Q { get; init; }
    public Platform? Platform { get; init; }
    public Status? Status { get; init; }
}

public sealed class ListGamesResult
{
    public required IReadOnlyList<GameListItem> Items { get; init; }
    public int TotalCount { get; init; }
}

public sealed class GameListItem
{
    public Guid Id { get; init; }
    public string Barcode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Platform Platform { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public Status Status { get; init; }
    public decimal Price { get; init; }
}

public sealed class GameDetail
{
    public Guid Id { get; init; }
    public string Barcode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Platform Platform { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public Status Status { get; init; }
    public decimal Price { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public sealed class CreateGameRequest
{
    public string Barcode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Platform Platform { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public Status Status { get; init; }
    public decimal Price { get; init; }
}

public sealed class UpdateGameRequest
{
    public string Barcode { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Platform Platform { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public Status Status { get; init; }
    public decimal Price { get; init; }
    public byte[] RowVersion { get; init; } = [];
}
