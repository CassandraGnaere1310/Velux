using System.Text.Json.Serialization;

namespace Velux.Shared.Models;

/// <summary>E.g. "Aluminium profile" or "Glass pane". Only alternatives in the same category are compared.</summary>
public class MaterialCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    // MaterialCategory 1 — classifies — 0..* MaterialAlternative
    [JsonIgnore]
    public List<MaterialAlternative> Alternatives { get; set; } = new();

    // MaterialCategory 1 — defines — 0..* TechnicalRequirement
    public List<TechnicalRequirement> Requirements { get; set; } = new();
}
