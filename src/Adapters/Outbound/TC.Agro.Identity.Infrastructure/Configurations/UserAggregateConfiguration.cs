using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TC.Agro.Identity.Domain.Aggregates;
using TC.Agro.Identity.Domain.ValueObjects;

namespace TC.Agro.Identity.Infrastructure.Configurations
{
    internal sealed class UserAggregateConfiguration : Configuration<UserAggregate>
    {
        public override void Configure(EntityTypeBuilder<UserAggregate> builder)
        {
            base.Configure(builder);
            builder.ToTable("users");

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200)
                .HasConversion(
                    email => email.Value.ToUpper(),
                    value => Email.FromDb(value)
                );

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(200)
                .HasConversion(
                    password => password.Hash,
                    value => Password.FromHash(value)
                );

            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion(
                    role => role.ToString(),
                    value => Role.Create(value)
                );

            builder.HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
