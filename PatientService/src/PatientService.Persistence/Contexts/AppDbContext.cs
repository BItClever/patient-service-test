using Microsoft.EntityFrameworkCore;
using PatientService.Core.Entities;

namespace PatientService.Persistence.Contexts;

public sealed class AppDbContext : DbContext
{
    public DbSet<Patient> Patients => Set<Patient>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
