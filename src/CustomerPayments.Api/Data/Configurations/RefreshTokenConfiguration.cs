using CustomerPayments.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerPayments.Api.Data.Configurations;

public sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(
        EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ReplacedByTokenHash)
            .HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash)
            .IsUnique();

        builder.HasIndex(x => x.UserId);
    }
}