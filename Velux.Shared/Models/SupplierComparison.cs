namespace Velux.Shared.Models;

public class SupplierComparison
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    // MaterialCategory 1 — scopes — 0..* SupplierComparison
    public int MaterialCategoryId { get; set; }
    public MaterialCategory? MaterialCategory { get; set; }

    // User 1 — creates — 0..* SupplierComparison (nullable until login is implemented)
    public int? CreatedByUserId { get; set; }
    public User? CreatedBy { get; set; }

    // SupplierComparison 1 ◆ includes 2..* ComparisonEntry
    public List<ComparisonEntry> Entries { get; set; } = new();
}
