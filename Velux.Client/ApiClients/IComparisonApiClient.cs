using Velux.Shared.Models;

namespace Velux.Client.ApiClients;

public interface IComparisonApiClient
{
    Task<List<SupplierComparison>> GetAllAsync();
    Task<SupplierComparison?> GetAsync(int comparisonId);
    Task<SupplierComparison> CalculateAsync(SupplierComparison comparison);
    Task<SupplierComparison> SaveAsync(SupplierComparison comparison);
}
