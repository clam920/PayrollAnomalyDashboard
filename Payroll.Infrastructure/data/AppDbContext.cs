using Microsoft.EntityFrameworkCore;
using Payroll.Domain.Entities;

namespace Payroll.Infrastructure.Data;

public class AppDbContext : DbContext
{
    // The constructor accepts configuration (like the database file path) from the API layer
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets represent your actual database tables
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<Anomaly> Anomalies => Set<Anomaly>();

    // Fluent API: This is where you configure database constraints (max lengths, required fields) 
    // without polluting your clean Domain entities with database-specific code.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>().HasKey(e => e.Id);
        modelBuilder.Entity<Timesheet>().HasKey(t => t.Id);
        modelBuilder.Entity<Anomaly>().HasKey(a => a.Id);
    }
}