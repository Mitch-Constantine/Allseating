using Allseating.Domain;
using Xunit;

namespace Allseating.Tests;

public class GameRulesTests
{
    [Theory]
    [InlineData("2025-01-01", "2025-01-02")] // today < release
    [InlineData("2025-06-01", "2025-12-31")]
    public void ValidateStatusAndReleaseDate_Upcoming_FutureDate_Valid(string today, string release)
    {
        var utcToday = DateOnly.Parse(today);
        var releaseDate = DateOnly.Parse(release);
        var result = GameRules.ValidateStatusAndReleaseDate(Status.Upcoming, releaseDate, utcToday);
        Assert.Null(result);
    }

    [Fact]
    public void ValidateStatusAndReleaseDate_Upcoming_NoDate_Invalid()
    {
        var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = GameRules.ValidateStatusAndReleaseDate(Status.Upcoming, null, utcToday);
        Assert.NotNull(result);
        Assert.Contains("required", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateStatusAndReleaseDate_Upcoming_ReleaseNotFuture_Invalid()
    {
        var utcToday = new DateOnly(2025, 6, 15);
        var releaseDate = new DateOnly(2025, 6, 14);
        var result = GameRules.ValidateStatusAndReleaseDate(Status.Upcoming, releaseDate, utcToday);
        Assert.NotNull(result);
        Assert.Contains("after", result);
    }

    [Theory]
    [InlineData("2025-06-15", "2025-06-14")]
    [InlineData("2025-06-15", "2025-06-15")]
    public void ValidateStatusAndReleaseDate_Active_OnOrBeforeToday_Valid(string today, string release)
    {
        var utcToday = DateOnly.Parse(today);
        var releaseDate = DateOnly.Parse(release);
        var result = GameRules.ValidateStatusAndReleaseDate(Status.Active, releaseDate, utcToday);
        Assert.Null(result);
    }

    [Fact]
    public void ValidateStatusAndReleaseDate_Active_NoDate_Invalid()
    {
        var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = GameRules.ValidateStatusAndReleaseDate(Status.Active, null, utcToday);
        Assert.NotNull(result);
        Assert.Contains("required", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateStatusAndReleaseDate_Active_FutureRelease_Invalid()
    {
        var utcToday = new DateOnly(2025, 6, 15);
        var releaseDate = new DateOnly(2025, 6, 16);
        var result = GameRules.ValidateStatusAndReleaseDate(Status.Active, releaseDate, utcToday);
        Assert.NotNull(result);
    }

    [Fact]
    public void ValidateStatusAndReleaseDate_Discontinued_NoDate_Valid()
    {
        var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = GameRules.ValidateStatusAndReleaseDate(Status.Discontinued, null, utcToday);
        Assert.Null(result);
    }

    [Fact]
    public void ValidateStatusAndReleaseDate_Discontinued_PastDate_Valid()
    {
        var utcToday = new DateOnly(2025, 6, 15);
        var releaseDate = new DateOnly(2024, 1, 1);
        var result = GameRules.ValidateStatusAndReleaseDate(Status.Discontinued, releaseDate, utcToday);
        Assert.Null(result);
    }

    [Fact]
    public void ValidateStatusAndReleaseDate_Discontinued_FutureDate_Invalid()
    {
        var utcToday = new DateOnly(2025, 6, 15);
        var releaseDate = new DateOnly(2025, 6, 16);
        var result = GameRules.ValidateStatusAndReleaseDate(Status.Discontinued, releaseDate, utcToday);
        Assert.NotNull(result);
    }

    [Fact]
    public void CanDelete_OnlyDiscontinued_True()
    {
        Assert.True(GameRules.CanDelete(Status.Discontinued));
    }

    [Theory]
    [InlineData(Status.Upcoming)]
    [InlineData(Status.Active)]
    public void CanDelete_NotDiscontinued_False(Status status)
    {
        Assert.False(GameRules.CanDelete(status));
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(99.99)]
    public void ValidatePrice_Positive_Valid(decimal price)
    {
        Assert.Null(GameRules.ValidatePrice(price));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void ValidatePrice_ZeroOrNegative_Invalid(decimal price)
    {
        var result = GameRules.ValidatePrice(price);
        Assert.NotNull(result);
        Assert.Contains("greater than 0", result, StringComparison.OrdinalIgnoreCase);
    }
}
