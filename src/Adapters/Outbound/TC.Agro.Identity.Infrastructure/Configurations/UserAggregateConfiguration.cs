namespace TC.Agro.Identity.Infrastructure.Configurations
{
    internal sealed class UserAggregateConfiguration : BaseEntityConfiguration<UserAggregate>
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

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(200);

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
                    role => role.Value,
                    value => Role.Create(value)
                );

            builder.HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
