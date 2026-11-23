using Velux.Shared.Models;

namespace Velux.Api.Data;

/// <summary>
/// Test data for the prototype.
/// ALL NUMBERS ARE ILLUSTRATIVE. Replace emission factors with values from real, public EPDs
/// (e.g. EPD Danmark or The International EPD System) and write the source in SourceReference.
/// Delete velux.db to re-seed after changing this file.
/// </summary>
public static class SeedData
{
    public static void Seed(AppDbContext db)
    {
        if (db.Suppliers.Any()) return;

        // --- Categories and technical requirements ---
        var aluminium = new MaterialCategory { Name = "Aluminium profile" };
        aluminium.Requirements.Add(new TechnicalRequirement
        {
            Property = "Tensile strength", ComparisonOperator = ComparisonOperator.GreaterOrEqual,
            RequiredValue = 160, Unit = "MPa"
        });

        var glass = new MaterialCategory { Name = "Glass pane" };
        glass.Requirements.Add(new TechnicalRequirement
        {
            Property = "Thermal transmittance (Ug)", ComparisonOperator = ComparisonOperator.LessOrEqual,
            RequiredValue = 1.1, Unit = "W/m2K"
        });

        // --- Suppliers ---
        var supplierA = new Supplier { Name = "Supplier A (current)", Country = "Germany", ContactInfo = "sales@supplier-a.example" };
        var supplierB = new Supplier { Name = "Supplier B", Country = "Norway", ContactInfo = "sales@supplier-b.example" };
        var supplierC = new Supplier { Name = "Supplier C (current)", Country = "Denmark", ContactInfo = "sales@supplier-c.example" };
        var supplierD = new Supplier { Name = "Supplier D", Country = "Poland", ContactInfo = "sales@supplier-d.example" };

        // --- Aluminium alternatives ---
        var standardAlu = Alternative("Standard aluminium alloy", "6063-T6", supplierA, aluminium,
            spec: ("Tensile strength", 215, "MPa"),
            factor: 8.6, unit: "kg", SourceType.Secondary, "ILLUSTRATIVE generic primary aluminium factor",
            "Industry average, not supplier-specific",
            price: 3.10m);

        var lowCarbonAlu = Alternative("Low-carbon aluminium alloy", "6063-T6", supplierB, aluminium,
            spec: ("Tensile strength", 205, "MPa"),
            factor: 4.0, unit: "kg", SourceType.Primary, "ILLUSTRATIVE supplier EPD",
            "Supplier-specific EPD, third-party verified",
            price: 3.45m);

        var recycledAlu = Alternative("Recycled aluminium alloy", "6060-T5", supplierB, aluminium,
            spec: ("Tensile strength", 150, "MPa"),   // fails the 160 MPa requirement on purpose
            factor: 2.3, unit: "kg", SourceType.Primary, "ILLUSTRATIVE supplier EPD",
            "Supplier-specific EPD, high recycled content",
            price: 3.30m);

        // --- Glass alternatives ---
        var standardGlass = Alternative("Standard double glazing", "4-16-4 Argon", supplierC, glass,
            spec: ("Thermal transmittance (Ug)", 1.1, "W/m2K"),
            factor: 25, unit: "m2", SourceType.Secondary, "ILLUSTRATIVE generic glazing factor",
            "Industry average",
            price: 38m);

        var lowCarbonGlass = Alternative("Low-carbon double glazing", "4-16-4 Argon", supplierD, glass,
            spec: ("Thermal transmittance (Ug)", 1.0, "W/m2K"),
            factor: 18, unit: "m2", SourceType.Primary, "ILLUSTRATIVE supplier PCF",
            "Supplier-specific PCF, not yet third-party verified",
            price: 42m);

        // --- Users (login is not implemented yet; these show the roles) ---
        var users = new[]
        {
            new User { Name = "Procurement demo user", Email = "procurement@velux.example", Role = UserRole.Procurement },
            new User { Name = "Sustainability demo user", Email = "sustainability@velux.example", Role = UserRole.Sustainability },
            new User { Name = "Supplier B representative", Email = "rep@supplier-b.example", Role = UserRole.SupplierRepresentative, Supplier = supplierB },
            new User { Name = "Administrator", Email = "admin@velux.example", Role = UserRole.Administrator }
        };

        db.AddRange(aluminium, glass, supplierA, supplierB, supplierC, supplierD);
        db.AddRange(standardAlu, lowCarbonAlu, recycledAlu, standardGlass, lowCarbonGlass);
        db.AddRange(users);
        db.SaveChanges();
    }

    private static MaterialAlternative Alternative(
        string name, string grade, Supplier supplier, MaterialCategory category,
        (string Property, double Value, string Unit) spec,
        double factor, string unit, SourceType sourceType, string sourceReference, string qualityNote,
        decimal price)
    {
        var alternative = new MaterialAlternative { Name = name, Grade = grade, Supplier = supplier, Category = category };

        alternative.Specifications.Add(new TechnicalSpecification
        {
            Property = spec.Property, Value = spec.Value, Unit = spec.Unit
        });

        alternative.EnvironmentalData.Add(new EnvironmentalData
        {
            EmissionFactor = factor,
            ReferenceUnit = unit,
            SystemBoundary = "Cradle-to-gate",
            SourceType = sourceType,
            SourceReference = sourceReference,
            QualityNote = qualityNote
        });

        alternative.Prices.Add(new MaterialPrice
        {
            Amount = price, Currency = "EUR", PriceUnit = unit, ValidFrom = new DateTime(2026, 1, 1)
        });

        return alternative;
    }
}
