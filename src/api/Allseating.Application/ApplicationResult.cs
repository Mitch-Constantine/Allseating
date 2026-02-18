namespace Allseating.Application;

public enum ConflictCode
{
    ConcurrencyConflict,
    InvalidState
}

public abstract record ApplicationResult<T>
{
    public sealed record Ok(T Value) : ApplicationResult<T>;
    public sealed record ValidationErrors(IReadOnlyList<string> Errors) : ApplicationResult<T>;
    public sealed record NotFound : ApplicationResult<T>;
    public sealed record Conflict(ConflictCode Code, string Message) : ApplicationResult<T>;
}
