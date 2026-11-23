using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Velux.Api.Data;
using Velux.Api.Services;
using Velux.Shared.Models;

namespace Velux.Tests;

/// <summary>Tests the comparison against a real (in-memory) SQLite database.</summary>
public class ComparisonServiceTests : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly AppDbContext db;

    public ComparisonServiceTests()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        db.Dispose();
        connection.Dispose();
    }

    private ComparisonService CreateService() =>
        new(db, new CO2eCalculator(), new CostCalculator(), new TechnicalAssessor());

    private static MaterialAlternative Alternative(string name, Supplier supplier, MaterialCategory category, double factor, decimal price)
    {
        var alternative = new MaterialAlternative { Name = name, Supplier = supplier, Category = category };
        alternative.EnvironmentalData.Add(new EnvironmentalData { EmissionFactor = factor, ReferenceUnit = "kg", SourceType = SourceType.Primary });
        alternative.Prices.Add(new MaterialPrice { Amount = price, Currency = "EUR", PriceUnit = "kg", ValidFrom = DateTime.Today });
        return alternative;
    }

    [Fact]
    public async Task Calculate_ComputesDifferencesAgainstBaseline()
    {
        var category = new MaterialCategory { Name = "Test category" };
        var supplier = new Supplier { Name = "Test supplier" };
        var current = Alternative("Current", supplier, category, factor: 10, price: 2.00m);
        var lowCarbon = Alternative("Low carbon", supplier, category, factor: 6, price: 2.50m);
        db.AddRange(category, supplier, current, lowCarbon);
        await db.SaveChangesAsync();

        var comparison = new SupplierComparison
        {
            Name = "Test",
            MaterialCategoryId = category.Id,
            Entries =
            {
                new ComparisonEntry { MaterialAlternativeId = current.Id, Quantity = 100, QuantityUnit = "kg", IsBaseline = true },
                new ComparisonEntry { MaterialAlternativeId = lowCarbon.Id, Quantity = 100, QuantityUnit = "kg" }
            }
        };

        var result = await CreateService().CalculateAsync(comparison);
        var low = result.Entries[1];

        Assert.Equal(600.0, low.TotalCO2e!.Value, 6);
        Assert.Equal(-400.0, low.CO2eDifference!.Value, 6);
        Assert.Equal(-40.0, low.CO2eDifferencePct!.Value, 6);
        Assert.Equal(50m, low.CostDifference!.Value);
    }

    [Fact]
    public async Task Validate_RejectsAlternativeFromAnotherCategory()
    {
        var aluminium = new MaterialCategory { Name = "Aluminium" };
        var glass = new MaterialCategory { Name = "Glass" };
        var supplier = new Supplier { Name = "Test supplier" };
        var alu = Alternative("Alu", supplier, aluminium, factor: 8, price: 3m);
        var pane = Alternative("Pane", supplier, glass, factor: 20, price: 40m);
        db.AddRange(aluminium, glass, supplier, alu, pane);
        await db.SaveChangesAsync();

        var comparison = new SupplierComparison
        {
            Name = "Mixed",
            MaterialCategoryId = aluminium.Id,
            Entries =
            {
                new ComparisonEntry { MaterialAlternativeId = alu.Id, Quantity = 1, QuantityUnit = "kg" },
                new ComparisonEntry { MaterialAlternativeId = pane.Id, Quantity = 1, QuantityUnit = "kg" }
            }
        };

        var errors = await CreateService().ValidateAsync(comparison, forSaving: true);

        Assert.Contains(errors, e => e.Contains("category"));
    }
}
