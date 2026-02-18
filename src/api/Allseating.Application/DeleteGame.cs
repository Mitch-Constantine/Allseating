using Allseating.Domain;

namespace Allseating.Application;

public class DeleteGame
{
    private readonly IGameRepository _repository;

    public DeleteGame(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApplicationResult<Unit>> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var game = await _repository.GetByIdForUpdateAsync(id, ct);
        if (game == null)
            return new ApplicationResult<Unit>.NotFound();

        if (!GameRules.CanDelete(game.Status))
            return new ApplicationResult<Unit>.Conflict(ConflictCode.InvalidState, "A game can only be deleted when its Status is Discontinued.");

        _repository.Remove(game);
        await _repository.SaveChangesAsync(ct);

        return new ApplicationResult<Unit>.Ok(default);
    }
}

public struct Unit { }
