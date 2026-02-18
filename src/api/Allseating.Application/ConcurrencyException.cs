namespace Allseating.Application;

/// <summary>
/// Thrown by infrastructure when a concurrency conflict is detected (e.g. RowVersion mismatch during save).
/// </summary>
public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}
