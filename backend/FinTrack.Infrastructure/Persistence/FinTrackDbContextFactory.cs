using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinTrack.Infrastructure.Persistence;

// Usado apenas pelas ferramentas do EF Core (dotnet ef) em tempo de design.
// Não conecta ao banco ao gerar migrations; o connection string real vem da configuração da API em runtime.
public class FinTrackDbContextFactory : IDesignTimeDbContextFactory<FinTrackDbContext>
{
    public FinTrackDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FinTrackDb")
            ?? "Host=localhost;Port=5432;Database=fintrack;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FinTrackDbContext(options);
    }
}
