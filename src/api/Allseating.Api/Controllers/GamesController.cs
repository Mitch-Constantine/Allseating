using Microsoft.AspNetCore.Mvc;
using Allseating.Application;
using Allseating.Domain;
using Allseating.Api.Dto;

namespace Allseating.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly ListGames _listGames;
    private readonly GetGame _getGame;
    private readonly CreateGame _createGame;
    private readonly UpdateGame _updateGame;
    private readonly DeleteGame _deleteGame;

    public GamesController(ListGames listGames, GetGame getGame, CreateGame createGame, UpdateGame updateGame, DeleteGame deleteGame)
    {
        _listGames = listGames;
        _getGame = getGame;
        _createGame = createGame;
        _updateGame = updateGame;
        _deleteGame = deleteGame;
    }

    [HttpGet]
    public async Task<ActionResult<GamesListResponse>> GetList(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10,
        [FromQuery] string? sortProp = null,
        [FromQuery] string? sortDir = null,
        [FromQuery] string? q = null,
        [FromQuery] string? platform = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var platformEnum = ParsePlatform(platform);
        var statusEnum = ParseStatus(status);
        if (platformEnum == null && !string.IsNullOrEmpty(platform))
            return BadRequest(new ProblemDetails { Status = 400, Title = "Validation Error", Detail = "Platform must be one of: PC, PS5, PS4, XBOX_SERIES, XBOX_ONE, SWITCH." });
        if (statusEnum == null && !string.IsNullOrEmpty(status))
            return BadRequest(new ProblemDetails { Status = 400, Title = "Validation Error", Detail = "Status must be one of: Upcoming, Active, Discontinued." });

        var request = new ListGamesRequest
        {
            Offset = offset,
            Limit = limit,
            SortProp = sortProp,
            SortDir = sortDir,
            Q = q,
            Platform = platformEnum,
            Status = statusEnum
        };

        var result = await _listGames.ExecuteAsync(request, ct);
        var items = result.Items.Select(x => new GameListItemDto(
            x.Id,
            x.Barcode,
            x.Title,
            x.Description,
            x.Platform.ToString(),
            x.ReleaseDate,
            x.Status.ToString(),
            x.Price)).ToList();

        return Ok(new GamesListResponse(items, result.TotalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GameDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        var game = await _getGame.ExecuteAsync(id, ct);
        if (game == null) return NotFound();
        return Ok(ToDetailDto(game));
    }

    [HttpPost]
    public async Task<ActionResult<GameDetailDto>> Create([FromBody] CreateGameDto dto, CancellationToken ct)
    {
        var parsed = ToCreateRequest(dto);
        if (parsed.Error != null) return BadRequest(new ProblemDetails { Status = 400, Title = "Validation Error", Detail = parsed.Error });

        var result = await _createGame.ExecuteAsync(parsed.Request!, ct);

        return result switch
        {
            ApplicationResult<GameDetail>.Ok ok => CreatedAtAction(nameof(GetById), new { id = ok.Value.Id }, ToDetailDto(ok.Value)),
            ApplicationResult<GameDetail>.ValidationErrors e => BadRequest(ValidationProblem(e.Errors)),
            _ => BadRequest()
        };
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GameDetailDto>> Update(Guid id, [FromBody] UpdateGameDto dto, CancellationToken ct)
    {
        var parsed = ToUpdateRequest(dto);
        if (parsed.Error != null) return BadRequest(new ProblemDetails { Status = 400, Title = "Validation Error", Detail = parsed.Error });

        var result = await _updateGame.ExecuteAsync(id, parsed.Request!, ct);

        return result switch
        {
            ApplicationResult<GameDetail>.Ok ok => Ok(ToDetailDto(ok.Value)),
            ApplicationResult<GameDetail>.ValidationErrors e => BadRequest(ValidationProblem(e.Errors)),
            ApplicationResult<GameDetail>.NotFound => NotFound(),
            ApplicationResult<GameDetail>.Conflict c => ConflictWithErrorCode(c.Code, c.Message),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _deleteGame.ExecuteAsync(id, ct);

        return result switch
        {
            ApplicationResult<Unit>.Ok => NoContent(),
            ApplicationResult<Unit>.NotFound => NotFound(),
            ApplicationResult<Unit>.Conflict c => ConflictWithErrorCode(c.Code, c.Message),
            _ => BadRequest()
        };
    }

    private static GameDetailDto ToDetailDto(GameDetail g) =>
        new(g.Id, g.Barcode, g.Title, g.Description, g.Platform.ToString(), g.ReleaseDate, g.Status.ToString(), g.Price, g.RowVersion);

    private ConflictObjectResult ConflictWithErrorCode(ConflictCode code, string message)
    {
        var problem = new ProblemDetails
        {
            Status = 409,
            Title = "Conflict",
            Detail = message
        };
        problem.Extensions["errorCode"] = code.ToString();
        return Conflict(problem);
    }

    private static object ValidationProblem(IReadOnlyList<string> errors)
    {
        var detail = errors.Count > 0 ? string.Join(" ", errors) : "Validation failed.";
        return new ProblemDetails
        {
            Status = 400,
            Title = "Validation Error",
            Detail = detail
        };
    }

    private static (CreateGameRequest? Request, string? Error) ToCreateRequest(CreateGameDto dto)
    {
        if (!Enum.TryParse<Platform>(dto.Platform, ignoreCase: true, out var platform) || !Enum.IsDefined(platform))
            return (null, "Platform must be one of: PC, PS5, PS4, XBOX_SERIES, XBOX_ONE, SWITCH.");
        if (!Enum.TryParse<Status>(dto.Status, ignoreCase: true, out var status) || !Enum.IsDefined(status))
            return (null, "Status must be one of: Upcoming, Active, Discontinued.");
        return (new CreateGameRequest
        {
            Barcode = dto.Barcode,
            Title = dto.Title,
            Description = dto.Description,
            Platform = platform,
            ReleaseDate = dto.ReleaseDate,
            Status = status,
            Price = dto.Price
        }, null);
    }

    private static (UpdateGameRequest? Request, string? Error) ToUpdateRequest(UpdateGameDto dto)
    {
        if (!Enum.TryParse<Platform>(dto.Platform, ignoreCase: true, out var platform) || !Enum.IsDefined(platform))
            return (null, "Platform must be one of: PC, PS5, PS4, XBOX_SERIES, XBOX_ONE, SWITCH.");
        if (!Enum.TryParse<Status>(dto.Status, ignoreCase: true, out var status) || !Enum.IsDefined(status))
            return (null, "Status must be one of: Upcoming, Active, Discontinued.");
        if (dto.RowVersion == null || dto.RowVersion.Length == 0)
            return (null, "RowVersion is required for updates.");
        return (new UpdateGameRequest
        {
            Barcode = dto.Barcode,
            Title = dto.Title,
            Description = dto.Description,
            Platform = platform,
            ReleaseDate = dto.ReleaseDate,
            Status = status,
            Price = dto.Price,
            RowVersion = dto.RowVersion
        }, null);
    }

    private static Platform? ParsePlatform(string? value) =>
        string.IsNullOrEmpty(value) ? null : Enum.TryParse<Platform>(value, ignoreCase: true, out var p) && Enum.IsDefined(p) ? p : null;

    private static Status? ParseStatus(string? value) =>
        string.IsNullOrEmpty(value) ? null : Enum.TryParse<Status>(value, ignoreCase: true, out var s) && Enum.IsDefined(s) ? s : null;
}
