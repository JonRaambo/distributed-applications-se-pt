using CarService.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarService.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Repair> Repairs => Set<Repair>();
    public DbSet<RepairDocument> RepairDocuments => Set<RepairDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasIndex(c => c.Phone).IsUnique();
        modelBuilder.Entity<Vehicle>().HasIndex(v => v.PlateNumber).IsUnique();

        modelBuilder.Entity<Customer>().Property(c => c.FirstName).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<Customer>().Property(c => c.LastName).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<Customer>().Property(c => c.Phone).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Customer>().Property(c => c.Email).HasMaxLength(80);

        modelBuilder.Entity<Vehicle>().Property(v => v.PlateNumber).HasMaxLength(10).IsRequired();
        modelBuilder.Entity<Vehicle>().Property(v => v.Brand).HasMaxLength(30).IsRequired();
        modelBuilder.Entity<Vehicle>().Property(v => v.Model).HasMaxLength(30).IsRequired();
        modelBuilder.Entity<Vehicle>().Property(v => v.Vin).HasMaxLength(17);

        modelBuilder.Entity<Repair>().Property(r => r.Title).HasMaxLength(80).IsRequired();
        modelBuilder.Entity<Repair>().Property(r => r.Description).HasMaxLength(500);

        modelBuilder.Entity<RepairDocument>().Property(d => d.FileName).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<RepairDocument>().Property(d => d.ContentType).HasMaxLength(80).IsRequired();
        modelBuilder.Entity<RepairDocument>().Property(d => d.StoredPath).HasMaxLength(260).IsRequired();
        modelBuilder.Entity<RepairDocument>().Property(d => d.Description).HasMaxLength(200);

        base.OnModelCreating(modelBuilder);
    }
}
