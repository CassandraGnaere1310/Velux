using System.Text.Json.Serialization;

namespace Velux.Shared.Models;

/// <summary>A rule every material in a category must meet, e.g. Tensile strength ≥ 160 MPa.</summary>
public class TechnicalRequirement
{
    public int Id { get; set; }
    public string Property { get; set; } = "";
    public ComparisonOperator ComparisonOperator { get; set; }
    public double RequiredValue { get; set; }
    public string Unit { get; set; } = "";

    public int MaterialCategoryId { get; set; }
    [JsonIgnore]
    public MaterialCategory? MaterialCategory { get; set; }
}
