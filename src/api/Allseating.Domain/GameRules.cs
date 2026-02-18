namespace Allseating.Domain;

/// <summary>
/// Business rules for Game. All "today" comparisons use the provided UTC calendar date.
/// </summary>
public static class GameRules
{
    public const int BarcodeMaxLength = 64;
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    /// <summary>
    /// Validates Status and ReleaseDate consistency using the current UTC calendar date.
    /// Returns null if valid, or an error message.
    /// </summary>
    public static string? ValidateStatusAndReleaseDate(Status status, DateOnly? releaseDate, DateOnly utcToday)
    {
        return status switch
        {
            Status.Upcoming => ValidateUpcoming(releaseDate, utcToday),
            Status.Active => ValidateActive(releaseDate, utcToday),
            Status.Discontinued => ValidateDiscontinued(releaseDate, utcToday),
            _ => "Invalid Status value."
        };
    }

    private static string? ValidateUpcoming(DateOnly? releaseDate, DateOnly utcToday)
    {
        if (!releaseDate.HasValue)
            return "ReleaseDate is required when Status is Upcoming.";
        if (releaseDate.Value <= utcToday)
            return "When Status is Upcoming, ReleaseDate must be after the current date.";
        return null;
    }

    private static string? ValidateActive(DateOnly? releaseDate, DateOnly utcToday)
    {
        if (!releaseDate.HasValue)
            return "ReleaseDate is required when Status is Active.";
        if (releaseDate.Value > utcToday)
            return "When Status is Active, ReleaseDate must be on or before the current date.";
        return null;
    }

    private static string? ValidateDiscontinued(DateOnly? releaseDate, DateOnly utcToday)
    {
        if (releaseDate.HasValue && releaseDate.Value > utcToday)
            return "When Status is Discontinued, ReleaseDate must not be in the future.";
        return null;
    }

    /// <summary>
    /// Returns true if the game may be deleted (only when Status == Discontinued).
    /// </summary>
    public static bool CanDelete(Status status) => status == Status.Discontinued;

    /// <summary>
    /// Validates that price is strictly greater than zero.
    /// Returns null if valid, or an error message.
    /// </summary>
    public static string? ValidatePrice(decimal price)
    {
        if (price <= 0)
            return "Price must be greater than 0.";
        return null;
    }
}
