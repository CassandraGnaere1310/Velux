using Microsoft.AspNetCore.Components.Forms;
using Velux.Shared.Models;

namespace Velux.Client.ApiClients;

public interface ISupplierApiClient
{
    Task<List<Supplier>> GetSuppliersAsync();
    Task<List<MaterialCategory>> GetCategoriesAsync();
    Task<List<MaterialAlternative>> GetAlternativesAsync(int categoryId);
    Task<MaterialAlternative?> GetAlternativeAsync(int alternativeId);
    Task<MaterialAlternative> SaveAlternativeAsync(MaterialAlternative alternative);
    Task UploadDocumentAsync(int environmentalDataId, IBrowserFile file, DocumentType documentType);
}
