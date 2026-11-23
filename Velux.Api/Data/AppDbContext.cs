using Microsoft.EntityFrameworkCore;
using Velux.Shared.Models;

namespace Velux.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<MaterialCategory> MaterialCategories => Set<MaterialCategory>();
    public DbSet<MaterialAlternative> MaterialAlternatives => Set<MaterialAlternative>();
    public DbSet<TechnicalSpecification> TechnicalSpecifications => Set<TechnicalSpecification>();
    public DbSet<TechnicalRequirement> TechnicalRequirements => Set<TechnicalRequirement>();
    public DbSet<EnvironmentalData> EnvironmentalDataRecords => Set<EnvironmentalData>();
    public DbSet<EnvironmentalDocument> EnvironmentalDocuments => Set<EnvironmentalDocument>();
    public DbSet<MaterialPrice> MaterialPrices => Set<MaterialPrice>();
    public DbSet<SupplierComparison> SupplierComparisons => Set<SupplierComparison>();
    public DbSet<ComparisonEntry> ComparisonEntries => Set<ComparisonEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(u => u.Supplier).WithMany()
            .HasForeignKey(u => u.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MaterialAlternative>()
            .HasOne(a => a.Supplier).WithMany(s => s.Alternatives)
            .HasForeignKey(a => a.SupplierId);

        modelBuilder.Entity<MaterialAlternative>()
            .HasOne(a => a.Category).WithMany(c => c.Alternatives)
            .HasForeignKey(a => a.MaterialCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupplierComparison>(c =>
        {
            c.HasOne(x => x.MaterialCategory).WithMany()
                .HasForeignKey(x => x.MaterialCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            c.HasOne(x => x.CreatedBy).WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            c.HasMany(x => x.Entries).WithOne(e => e.SupplierComparison)
                .HasForeignKey(e => e.SupplierComparisonId)
                .OnDelete(DeleteBehavior.Cascade); // composition: entries die with the comparison
        });

        modelBuilder.Entity<ComparisonEntry>(e =>
        {
            e.HasOne(x => x.MaterialAlternative).WithMany()
                .HasForeignKey(x => x.MaterialAlternativeId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.EnvironmentalData).WithMany()
                .HasForeignKey(x => x.EnvironmentalDataId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.MaterialPrice).WithMany()
                .HasForeignKey(x => x.MaterialPriceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Derived values are calculated, not stored
            e.Ignore(x => x.TotalCO2e);
            e.Ignore(x => x.TotalCost);
            e.Ignore(x => x.CO2eDifference);
            e.Ignore(x => x.CO2eDifferencePct);
            e.Ignore(x => x.CostDifference);
            e.Ignore(x => x.TechnicalAssessments);
        });
    }
}
