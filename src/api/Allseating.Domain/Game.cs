namespace Allseating.Domain;

public class Game
{
    public Guid Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Platform Platform { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public Status Status { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
