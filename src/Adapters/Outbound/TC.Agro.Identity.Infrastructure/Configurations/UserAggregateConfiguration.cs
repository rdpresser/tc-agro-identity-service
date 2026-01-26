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

            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("email")
                    .IsRequired()
                    .HasMaxLength(200);

                email.HasIndex(e => e.Value)
                    .IsUnique();
            });

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

            builder.OwnsOne(u => u.Role, role =>
            {
                role.Property(r => r.Value)
                    .HasColumnName("role")
                    .IsRequired()
                    .HasMaxLength(20);
            });

            builder.Navigation(u => u.Email).IsRequired();
            builder.Navigation(u => u.Role).IsRequired();

            ////builder.HasIndex("Email_Value")
            ////    .IsUnique();
        }
    }
}
