using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace project_pharmacie.Data;

public class PharmacieDbContextFactory : IDesignTimeDbContextFactory<PharmacieDbContext>
{
    public PharmacieDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PharmacieDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=.;Database=PharmacieDb;Trusted_Connection=True;TrustServerCertificate=True"
        );

        return new PharmacieDbContext(optionsBuilder.Options);
    }
}