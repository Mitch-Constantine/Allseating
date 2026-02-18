using Allseating.Domain;

namespace Allseating.Application;

public class CreateGame
{
    private readonly IGameRepository _repository;
    private readonly IClock _clock;

    public CreateGame(IGameRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<ApplicationResult<GameDetail>> ExecuteAsync(CreateGameRequest request, CancellationToken ct = default)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
            return new ApplicationResult<GameDetail>.ValidationErrors(errors);

        if (await _repository.ExistsBarcodeAsync(request.Barcode, null, ct))
            return new ApplicationResult<GameDetail>.ValidationErrors(["A game with this Barcode already exists."]);

        var utcNow = _clock.UtcNow;
        var utcToday = DateOnly.FromDateTime(utcNow);
        var statusError = GameRules.ValidateStatusAndReleaseDate(request.Status, request.ReleaseDate, utcToday);
        if (statusError != null)
            return new ApplicationResult<GameDetail>.ValidationErrors([statusError]);

        var game = new Game
        {
            Id = Guid.NewGuid(),
            Barcode = request.Barcode.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Platform = request.Platform,
            ReleaseDate = request.ReleaseDate,
            Status = request.Status,
            Price = request.Price,
            CreatedUtc = utcNow,
            UpdatedUtc = utcNow,
            RowVersion = []
        };

        _repository.Add(game);
        try
        {
            await _repository.SaveChangesAsync(ct);
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

    private static List<string> Validate(CreateGameRequest request)
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

        return errors;
    }
}
