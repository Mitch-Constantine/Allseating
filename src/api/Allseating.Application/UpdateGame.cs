using Allseating.Domain;

namespace Allseating.Application;

public class UpdateGame
{
    private readonly IGameRepository _repository;
    private readonly IClock _clock;

    public UpdateGame(IGameRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<ApplicationResult<GameDetail>> ExecuteAsync(Guid id, UpdateGameRequest request, CancellationToken ct = default)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
            return new ApplicationResult<GameDetail>.ValidationErrors(errors);

        var game = await _repository.GetByIdForUpdateAsync(id, ct);
        if (game == null)
            return new ApplicationResult<GameDetail>.NotFound();

        if (!game.RowVersion.SequenceEqual(request.RowVersion))
            return new ApplicationResult<GameDetail>.Conflict(ConflictCode.ConcurrencyConflict, "This game was modified elsewhere. Reload the latest version.");

        if (await _repository.ExistsBarcodeAsync(request.Barcode.Trim(), id, ct))
            return new ApplicationResult<GameDetail>.ValidationErrors(["A game with this Barcode already exists."]);

        var utcToday = DateOnly.FromDateTime(_clock.UtcNow);
        var statusError = GameRules.ValidateStatusAndReleaseDate(request.Status, request.ReleaseDate, utcToday);
        if (statusError != null)
            return new ApplicationResult<GameDetail>.ValidationErrors([statusError]);

        game.Barcode = request.Barcode.Trim();
        game.Title = request.Title.Trim();
        game.Description = request.Description.Trim();
        game.Platform = request.Platform;
        game.ReleaseDate = request.ReleaseDate;
        game.Status = request.Status;
        game.Price = request.Price;
        game.UpdatedUtc = _clock.UtcNow;

        try
        {
            await _repository.SaveChangesAsync(ct);
        }
        catch (ConcurrencyException)
        {
            return new ApplicationResult<GameDetail>.Conflict(ConflictCode.ConcurrencyConflict, "This game was modified elsewhere. Reload the latest version.");
        }
        catch (DuplicateBarcodeException)
        {
            return new ApplicationResult<GameDetail>.ValidationErrors(["A game with this Barcode already exists."]);
        }

        return new ApplicationResult<GameDetail>.Ok(new GameDetail
        {
            Id = game.Id,
            Barcode = game.Barcode,
            Title = game.Title,
            Description = game.Description,
            Platform = game.Platform,
            ReleaseDate = game.ReleaseDate,
            Status = game.Status,
            Price = game.Price,
            RowVersion = game.RowVersion
        });
    }

    private static List<string> Validate(UpdateGameRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Barcode))
            errors.Add("Barcode is required.");
        else if (request.Barcode.Length > GameRules.BarcodeMaxLength)
            errors.Add($"Barcode must not exceed {GameRules.BarcodeMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(request.Title))
            errors.Add("Title is required.");
        else if (request.Title.Length > GameRules.TitleMaxLength)
            errors.Add($"Title must not exceed {GameRules.TitleMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add("Description is required.");
        else if (request.Description.Length > GameRules.DescriptionMaxLength)
            errors.Add($"Description must not exceed {GameRules.DescriptionMaxLength} characters.");

        if (!Enum.IsDefined(typeof(Platform), request.Platform))
            errors.Add("Platform must be one of: PC, PS5, PS4, XBOX_SERIES, XBOX_ONE, SWITCH.");

        if (!Enum.IsDefined(typeof(Status), request.Status))
            errors.Add("Status must be one of: Upcoming, Active, Discontinued.");

        var priceError = GameRules.ValidatePrice(request.Price);
        if (priceError != null)
            errors.Add(priceError);

        if (request.RowVersion == null || request.RowVersion.Length == 0)
            errors.Add("RowVersion is required for updates.");

        return errors;
    }
}
