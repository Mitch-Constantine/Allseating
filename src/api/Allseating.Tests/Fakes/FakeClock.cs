using Allseating.Application;

namespace Allseating.Tests.Fakes;

public sealed class FakeClock : IClock
{
    public DateTime UtcNow { get; set; } = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
}
