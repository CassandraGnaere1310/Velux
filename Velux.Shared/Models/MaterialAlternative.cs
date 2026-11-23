namespace Velux.Shared.Models;

/// <summary>A specific material offered by a supplier, e.g. a low-carbon aluminium alloy.</summary>
public class MaterialAlternative
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    /// <summary>The specific variant or quality, e.g. alloy designation or glass type.</summary>
    public string Grade { get; set; } = "";

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int MaterialCategoryId { get; set; }
    public MaterialCategory? Category { get; set; }

    // MaterialAlternative 1 — has — 0..* (each)
    public List<TechnicalSpecification> Specifications { get; set; } = new();
    public List<EnvironmentalData> EnvironmentalData { get; set; } = new();
    public List<MaterialPrice> Prices { get; set; } = new();
}
