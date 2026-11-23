using Velux.Shared.Models;

namespace Velux.Api.Services;

public interface IComparisonService
{
    Task<List<SupplierComparison>> GetAllAsync();
    Task<SupplierComparison?> GetAsync(int id);
    Task<List<string>> ValidateAsync(SupplierComparison comparison, bool forSaving);
    Task<SupplierComparison> CalculateAsync(SupplierComparison comparison);
    Task<SupplierComparison> SaveAsync(SupplierComparison comparison);
}
