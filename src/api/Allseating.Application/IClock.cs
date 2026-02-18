namespace Allseating.Application;

public interface IClock
{
    DateTime UtcNow { get; }
}
