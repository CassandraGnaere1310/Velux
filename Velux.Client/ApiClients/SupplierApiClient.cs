using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Velux.Shared.Models;

namespace Velux.Client.ApiClients;

public class SupplierApiClient(HttpClient http) : ISupplierApiClient
{
    private const long MaxUploadBytes = 20_000_000;

    public async Task<List<Supplier>> GetSuppliersAsync() =>
        await http.GetFromJsonAsync<List<Supplier>>("api/suppliers", JsonDefaults.Options) ?? new();

    public async Task<List<MaterialCategory>> GetCategoriesAsync() =>
        await http.GetFromJsonAsync<List<MaterialCategory>>("api/categories", JsonDefaults.Options) ?? new();

    public async Task<List<MaterialAlternative>> GetAlternativesAsync(int categoryId) =>
        await http.GetFromJsonAsync<List<MaterialAlternative>>($"api/alternatives?categoryId={categoryId}", JsonDefaults.Options) ?? new();

    public Task<MaterialAlternative?> GetAlternativeAsync(int alternativeId) =>
        http.GetFromJsonAsync<MaterialAlternative>($"api/alternatives/{alternativeId}", JsonDefaults.Options);

    /// <summary>Creates (POST) a new alternative or updates (PUT) an existing one.</summary>
    public async Task<MaterialAlternative> SaveAlternativeAsync(MaterialAlternative alternative)
    {
        var response = alternative.Id == 0
            ? await http.PostAsJsonAsync("api/alternatives", alternative, JsonDefaults.Options)
            : await http.PutAsJsonAsync($"api/alternatives/{alternative.Id}", alternative, JsonDefaults.Options);

        await response.EnsureSuccessOrThrowAsync();
        return (await response.Content.ReadFromJsonAsync<MaterialAlternative>(JsonDefaults.Options))!;
    }

    public async Task UploadDocumentAsync(int environmentalDataId, IBrowserFile file, DocumentType documentType)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(file.OpenReadStream(MaxUploadBytes)), "file", file.Name);
        content.Add(new StringContent(file.Name), "title");
        content.Add(new StringContent(documentType.ToString()), "documentType");

        var response = await http.PostAsync($"api/environmentaldata/{environmentalDataId}/documents", content);
        await response.EnsureSuccessOrThrowAsync();
    }
}
