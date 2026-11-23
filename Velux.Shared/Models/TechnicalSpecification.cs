using System.Text.Json.Serialization;

namespace Velux.Shared.Models;

/// <summary>A measured property of a material, e.g. Tensile strength = 215 MPa.</summary>
public class TechnicalSpecification
{
    public int Id { get; set; }
    public string Property { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";

    public int MaterialAlternativeId { get; set; }
    [JsonIgnore]
    public MaterialAlternative? MaterialAlternative { get; set; }
}
