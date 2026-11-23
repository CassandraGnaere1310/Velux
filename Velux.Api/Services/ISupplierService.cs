using Velux.Shared.Models;

namespace Velux.Api.Services;

public interface ISupplierService
{
    Task<List<Supplier>> GetSuppliersAsync();
    Task<List<MaterialCategory>> GetCategoriesAsync();
    Task<List<MaterialAlternative>> GetAlternativesAsync(int categoryId);
    Task<MaterialAlternative?> GetAlternativeAsync(int id);
    Task<List<string>> ValidateAsync(MaterialAlternative alternative);
    Task<MaterialAlternative> CreateAlternativeAsync(MaterialAlternative alternative);
    Task<MaterialAlternative?> UpdateAlternativeAsync(int id, MaterialAlternative alternative);
    Task<EnvironmentalDocument?> AddDocumentAsync(int environmentalDataId, string title, DocumentType type, string filePath);
}
