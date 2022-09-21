using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OneTimePassGen.Domain.Entities;

namespace OneTimePassGen.Infrastructure.Persistance.Configurations;

internal sealed class UserGeneratedPasswordConfiguration : IEntityTypeConfiguration<UserGeneratedPassword>
{
    public void Configure(EntityTypeBuilder<UserGeneratedPassword> builder)
    {
        builder.ToTable("UserGeneratedPasswords");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.Password)
            .IsRequired();

        builder.Property(t => t.ExpiersOn)
            .IsRequired();
    }
}
