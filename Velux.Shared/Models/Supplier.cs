namespace Velux.Shared.Models;

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Country { get; set; } = "";
    public string ContactInfo { get; set; } = "";

    // Supplier 1 — offers — 0..* MaterialAlternative
    public List<MaterialAlternative> Alternatives { get; set; } = new();
}
