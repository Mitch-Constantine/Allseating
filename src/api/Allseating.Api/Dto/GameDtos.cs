using Allseating.Domain;

namespace Allseating.Api.Dto;

public record GameListItemDto(
    Guid Id,
    string Barcode,
    string Title,
    string Description,
    string Platform,
    DateOnly? ReleaseDate,
    string Status,
    decimal Price);

public record GamesListResponse(IReadOnlyList<GameListItemDto> Items, int TotalCount);

public record GameDetailDto(
    Guid Id,
    string Barcode,
    string Title,
    string Description,
    string Platform,
    DateOnly? ReleaseDate,
    string Status,
    decimal Price,
    byte[] RowVersion);

public record CreateGameDto(
    string Barcode,
    string Title,
    string Description,
    string Platform,
    DateOnly? ReleaseDate,
    string Status,
    decimal Price);

public record UpdateGameDto(
    string Barcode,
    string Title,
    string Description,
    string Platform,
    DateOnly? ReleaseDate,
    string Status,
    decimal Price,
    byte[] RowVersion);
