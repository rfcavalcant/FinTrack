using FinTrack.Domain.Goals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();
        builder.Ignore(g => g.DomainEvents);

        builder.Property(g => g.UserId).IsRequired();

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.OwnsOne(g => g.Target, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TargetAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("TargetCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(g => g.CurrentAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CurrentAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("CurrentCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(g => g.Deadline);

        builder.Property(g => g.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(g => g.UserId);
    }
}
