namespace Allseating.Infrastructure.Seed;

public sealed class SeedFileDto
{
    public int Version { get; set; }
    public List<SeedGameDto> Games { get; set; } = [];
}

public sealed class SeedGameDto
{
    public string Barcode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string? ReleaseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
