using System.Text.Json.Serialization;

namespace Velux.Shared.Models;

/// <summary>A CO2e emission factor for a material, with its source and quality.</summary>
public class EnvironmentalData
{
    public int Id { get; set; }

    /// <summary>kg CO2e per <see cref="ReferenceUnit"/>.</summary>
    public double EmissionFactor { get; set; }

    /// <summary>The unit the factor refers to, e.g. "kg" or "m2". Must match the quantity unit in a comparison.</summary>
    public string ReferenceUnit { get; set; } = "kg";

    /// <summary>E.g. "Cradle-to-gate".</summary>
    public string SystemBoundary { get; set; } = "Cradle-to-gate";

    public SourceType SourceType { get; set; }
    public string SourceReference { get; set; } = "";
    public string QualityNote { get; set; } = "";

    public int MaterialAlternativeId { get; set; }
    [JsonIgnore]
    public MaterialAlternative? MaterialAlternative { get; set; }

    // EnvironmentalData 0..* — is supported by — 0..* EnvironmentalDocument
    public List<EnvironmentalDocument> Documents { get; set; } = new();
}
