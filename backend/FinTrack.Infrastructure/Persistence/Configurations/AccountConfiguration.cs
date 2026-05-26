using FinTrack.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Ignore(a => a.DomainEvents);

        builder.Property(a => a.UserId).IsRequired();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Money (obrigatório) como owned type → colunas Balance / BalanceCurrency.
        builder.OwnsOne(a => a.Balance, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Balance")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("BalanceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Money? (opcional) — colunas anuláveis; sem IsRequired para o EF tratar o owned como nulo.
        builder.OwnsOne(a => a.CreditLimit, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("CreditLimit")
                .HasPrecision(18, 2);

            money.Property(m => m.Currency)
                .HasColumnName("CreditLimitCurrency")
                .HasMaxLength(3);
        });

        builder.Property(a => a.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(a => a.UserId);
    }
}
