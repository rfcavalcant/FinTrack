using FinTrack.Domain.Budgeting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();
        builder.Ignore(b => b.DomainEvents);

        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.CategoryId).IsRequired();

        builder.OwnsOne(b => b.Period, period =>
        {
            period.Property(p => p.Start)
                .HasColumnName("PeriodStart")
                .IsRequired();

            period.Property(p => p.End)
                .HasColumnName("PeriodEnd")
                .IsRequired();
        });

        builder.OwnsOne(b => b.Limit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("LimitAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("LimitCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(b => b.Consumption, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ConsumptionAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("ConsumptionCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.HasIndex(b => new { b.UserId, b.CategoryId });
    }
}
