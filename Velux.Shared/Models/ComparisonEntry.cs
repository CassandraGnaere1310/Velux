using System.Text.Json.Serialization;

namespace Velux.Shared.Models;

/// <summary>One line in a comparison: a material alternative, a quantity, and the calculated results.</summary>
public class ComparisonEntry
{
    public int Id { get; set; }

    public int SupplierComparisonId { get; set; }
    [JsonIgnore]
    public SupplierComparison? SupplierComparison { get; set; }

    // ComparisonEntry 0..* — evaluates — 1 MaterialAlternative
    public int MaterialAlternativeId { get; set; }
    public MaterialAlternative? MaterialAlternative { get; set; }

    // ComparisonEntry 0..* — uses — 0..1 EnvironmentalData / MaterialPrice
    // Stored so it is always clear which data a result was calculated from.
    public int? EnvironmentalDataId { get; set; }
    public EnvironmentalData? EnvironmentalData { get; set; }
    public int? MaterialPriceId { get; set; }
    public MaterialPrice? MaterialPrice { get; set; }

    public double Quantity { get; set; }
    public string QuantityUnit { get; set; } = "kg";

    /// <summary>
    /// In the domain model the baseline is the association "selects as baseline".
    /// It is implemented as a flag to avoid a circular foreign key between the two tables.
    /// </summary>
    public bool IsBaseline { get; set; }

    // Derived values (/ in the domain model). Calculated by ComparisonService, not stored in the database.
    public double? TotalCO2e { get; set; }
    public decimal? TotalCost { get; set; }
    public double? CO2eDifference { get; set; }
    public double? CO2eDifferencePct { get; set; }
    public decimal? CostDifference { get; set; }

    // ComparisonEntry 1 ◆ records 0..* TechnicalAssessment (calculated, not stored)
    public List<TechnicalAssessment> TechnicalAssessments { get; set; } = new();
}
