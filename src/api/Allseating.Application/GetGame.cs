namespace Allseating.Application;

public class GetGame
{
    private readonly IGameRepository _repository;

    public GetGame(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<GameDetail?> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var game = await _repository.GetByIdAsync(id, ct);
        if (game == null) return null;

        return new GameDetail
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
        };
    }
}
