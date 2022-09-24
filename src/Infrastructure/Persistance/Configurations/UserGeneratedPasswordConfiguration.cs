using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OneTimePassGen.Domain.Entities;

namespace OneTimePassGen.Infrastructure.Persistance.Configurations;

internal sealed class UserGeneratedPasswordConfiguration : IEntityTypeConfiguration<UserGeneratedPassword>
{
    public void Configure(EntityTypeBuilder<UserGeneratedPassword> builder)
    {
        builder.ToTable("UserGeneratedPasswords");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Password)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.ExpiersAt)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();
    }
}