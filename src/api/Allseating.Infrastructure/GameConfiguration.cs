using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Allseating.Domain;

namespace Allseating.Infrastructure;

public sealed class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Barcode).HasMaxLength(64).IsRequired();
        builder.HasIndex(e => e.Barcode).IsUnique();

        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.Platform).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(e => e.ReleaseDate).HasConversion<DateOnly?>();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(e => e.Price).HasPrecision(10, 2).IsRequired();
        builder.Property(e => e.CreatedUtc).IsRequired();
        builder.Property(e => e.UpdatedUtc).IsRequired();
        builder.Property(e => e.RowVersion).IsRowVersion();
    }
}
