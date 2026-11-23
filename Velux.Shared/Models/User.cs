namespace Velux.Shared.Models;

/// <summary>A person who logs in: a VELUX employee or a supplier representative.</summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public UserRole Role { get; set; }

    // User 0..* — represents — 0..1 Supplier (only for supplier representatives)
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
}
