using System.Text.Json;
using Microsoft.Extensions.Logging;
using Allseating.Application;
using Allseating.Domain;

namespace Allseating.Infrastructure.Seed;

public sealed class SeedService
{
    private readonly IGameRepository _repository;
    private readonly IClock _clock;
    private readonly ILogger<SeedService> _logger;

    public SeedService(IGameRepository repository, IClock clock, ILogger<SeedService> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Seeds from JSON file if the Games table is empty. Invalid items are logged and skipped.
    /// </summary>
    public async Task SeedFromFileIfEmptyAsync(string seedFilePath, CancellationToken ct = default)
    {
        if (!File.Exists(seedFilePath))
            return;

        var count = await _repository.CountAsync(ct);
        if (count > 0)
            return;

        await using var stream = File.OpenRead(seedFilePath);
        SeedFileDto? dto;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            dto = await JsonSerializer.DeserializeAsync<SeedFileDto>(stream, options, cancellationToken: ct);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Seed file at {Path} is invalid JSON", seedFilePath);
            return;
        }

        if (dto?.Games == null || dto.Games.Count == 0)
            return;

        if (dto.Version != 1)
        {
            _logger.LogError("Seed file version {Version} is not supported (expected 1)", dto.Version);
            return;
        }

        var utcNow = _clock.UtcNow;
        var utcToday = DateOnly.FromDateTime(utcNow);
        var seenBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var added = 0;

        foreach (var item in dto.Games)
        {
            var (game, error) = TryParseGame(item, utcToday, seenBarcodes);
            if (error != null)
            {
                _logger.LogError("Seed item invalid (barcode: {Barcode}): {Error}", item.Barcode ?? "(null)", error);
                continue;
            }

            if (game != null)
            {
                seenBarcodes.Add(game.Barcode);
                _repository.Add(game);
                added++;
            }
        }

        await _repository.SaveChangesAsync(ct);
    }

    private (Game? Game, string? Error) TryParseGame(SeedGameDto item, DateOnly utcToday, HashSet<string> seenBarcodes)
    {
        if (string.IsNullOrWhiteSpace(item.Barcode))
            return (null, "Barcode is required.");
        if (item.Barcode.Length > GameRules.BarcodeMaxLength)
            return (null, $"Barcode must not exceed {GameRules.BarcodeMaxLength} characters.");
        if (seenBarcodes.Contains(item.Barcode))
            return (null, "Duplicate barcode in seed file.");

        if (string.IsNullOrWhiteSpace(item.Title))
            return (null, "Title is required.");
        if (item.Title.Length > GameRules.TitleMaxLength)
            return (null, $"Title must not exceed {GameRules.TitleMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(item.Description))
            return (null, "Description is required.");
        if (item.Description.Length > GameRules.DescriptionMaxLength)
            return (null, $"Description must not exceed {GameRules.DescriptionMaxLength} characters.");

        if (!Enum.TryParse<Platform>(item.Platform, ignoreCase: true, out var platform) || !Enum.IsDefined(platform))
            return (null, "Invalid Platform.");

        if (!Enum.TryParse<Status>(item.Status, ignoreCase: true, out var status) || !Enum.IsDefined(status))
            return (null, "Invalid Status.");

        DateOnly? releaseDate = null;
        if (!string.IsNullOrWhiteSpace(item.ReleaseDate))
        {
            if (!DateOnly.TryParse(item.ReleaseDate, out var d))
                return (null, "ReleaseDate must be YYYY-MM-DD.");
            releaseDate = d;
        }

        var statusError = GameRules.ValidateStatusAndReleaseDate(status, releaseDate, utcToday);
        if (statusError != null)
            return (null, statusError);

        var priceError = GameRules.ValidatePrice(item.Price);
        if (priceError != null)
            return (null, priceError);

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Barcode = item.Barcode.Trim(),
            Title = item.Title.Trim(),
            Description = item.Description.Trim(),
            Platform = platform,
            ReleaseDate = releaseDate,
            Status = status,
            Price = item.Price,
            CreatedUtc = _clock.UtcNow,
            UpdatedUtc = _clock.UtcNow,
            RowVersion = []
        };

        return (game, null);
    }
}
