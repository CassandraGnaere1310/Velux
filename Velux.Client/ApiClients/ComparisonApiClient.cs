using System.Net.Http.Json;
using Velux.Shared.Models;

namespace Velux.Client.ApiClients;

public class ComparisonApiClient(HttpClient http) : IComparisonApiClient
{
    public async Task<List<SupplierComparison>> GetAllAsync() =>
        await http.GetFromJsonAsync<List<SupplierComparison>>("api/comparisons", JsonDefaults.Options) ?? new();

    public Task<SupplierComparison?> GetAsync(int comparisonId) =>
        http.GetFromJsonAsync<SupplierComparison>($"api/comparisons/{comparisonId}", JsonDefaults.Options);

    public async Task<SupplierComparison> CalculateAsync(SupplierComparison comparison)
    {
        var response = await http.PostAsJsonAsync("api/comparisons/calculate", comparison, JsonDefaults.Options);
        await response.EnsureSuccessOrThrowAsync();
        return (await response.Content.ReadFromJsonAsync<SupplierComparison>(JsonDefaults.Options))!;
    }

    public async Task<SupplierComparison> SaveAsync(SupplierComparison comparison)
    {
        var response = await http.PostAsJsonAsync("api/comparisons", comparison, JsonDefaults.Options);
        await response.EnsureSuccessOrThrowAsync();
        return (await response.Content.ReadFromJsonAsync<SupplierComparison>(JsonDefaults.Options))!;
    }
}
