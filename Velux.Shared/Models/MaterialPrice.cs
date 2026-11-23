using System.Text.Json.Serialization;

namespace Velux.Shared.Models;

/// <summary>A price for a material. Several prices over time are kept as history (ValidFrom).</summary>
public class MaterialPrice
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EUR";

    /// <summary>The unit the price refers to, e.g. "kg" or "m2".</summary>
    public string PriceUnit { get; set; } = "kg";
    public DateTime ValidFrom { get; set; } = DateTime.Today;

    public int MaterialAlternativeId { get; set; }
    [JsonIgnore]
    public MaterialAlternative? MaterialAlternative { get; set; }
}
