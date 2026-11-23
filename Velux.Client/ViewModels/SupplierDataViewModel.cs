using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components.Forms;
using Velux.Client.ApiClients;
using Velux.Shared.Models;

namespace Velux.Client.ViewModels;

/// <summary>
/// ViewModel for SupplierDataPage (UC1 Submit and Update Supplier Information).
/// Holds the screen state; the View only binds to these properties and calls these methods.
/// </summary>
public partial class SupplierDataViewModel(ISupplierApiClient api) : ObservableObject
{
    [ObservableProperty] private List<Supplier> suppliers = new();
    [ObservableProperty] private List<MaterialCategory> categories = new();
    [ObservableProperty] private MaterialAlternative? selectedAlternative;
    [ObservableProperty] private List<string> validationErrors = new();
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private bool isBusy;

    // SSD: openSupplierInformation()
    public async Task LoadSuppliersAsync()
    {
        IsBusy = true;
        try
        {
            Categories = await api.GetCategoriesAsync();
            Suppliers = await api.GetSuppliersAsync();
        }
        catch (Exception ex)
        {
            ValidationErrors = new() { $"Could not reach the API. Is Velux.Api running? ({ex.Message})" };
        }
        finally
        {
            IsBusy = false;
        }
    }

    // SSD: selectMaterial(materialId) – UC1 branch 2a–2b
    public async Task LoadAlternativeAsync(int alternativeId)
    {
        ResetMessages();
        SelectedAlternative = await api.GetAlternativeAsync(alternativeId);
    }

    // SSD: createMaterial()
    public void CreateAlternative(int supplierId)
    {
        ResetMessages();
        SelectedAlternative = new MaterialAlternative
        {
            SupplierId = supplierId,
            MaterialCategoryId = Categories.FirstOrDefault()?.Id ?? 0,
            EnvironmentalData = { new EnvironmentalData { ReferenceUnit = "kg", SystemBoundary = "Cradle-to-gate", SourceType = SourceType.Primary } },
            Prices = { new MaterialPrice { Currency = "EUR", PriceUnit = "kg", ValidFrom = DateTime.Today } }
        };
    }

    // SSD: enterMaterialDetails(...) and enterPricesAndCO2e(...) happen through data binding in the View.
    public void AddSpecification() =>
        SelectedAlternative?.Specifications.Add(new TechnicalSpecification());

    public void AddEnvironmentalData() =>
        SelectedAlternative?.EnvironmentalData.Add(new EnvironmentalData { ReferenceUnit = "kg", SystemBoundary = "Cradle-to-gate" });

    public void AddPrice() =>
        SelectedAlternative?.Prices.Add(new MaterialPrice { Currency = "EUR", PriceUnit = "kg", ValidFrom = DateTime.Today });

    // SSD: submitInformation() – UC1 steps 7–10, exception 8a–8c
    public async Task SaveAlternativeAsync()
    {
        if (SelectedAlternative is null) return;

        ResetMessages();
        ValidationErrors = Validate(SelectedAlternative);
        if (ValidationErrors.Count > 0) return;

        IsBusy = true;
        try
        {
            SelectedAlternative = await api.SaveAlternativeAsync(SelectedAlternative);
            Suppliers = await api.GetSuppliersAsync();
            StatusMessage = "The information was saved.";
        }
        catch (ApiException ex)
        {
            ValidationErrors = ex.Errors;
        }
        finally
        {
            IsBusy = false;
        }
    }

    // SSD: uploadDocument(file, documentType)
    public async Task UploadDocumentAsync(int environmentalDataId, IBrowserFile file, DocumentType documentType)
    {
        ResetMessages();
        IsBusy = true;
        try
        {
            await api.UploadDocumentAsync(environmentalDataId, file, documentType);
            if (SelectedAlternative is not null)
                SelectedAlternative = await api.GetAlternativeAsync(SelectedAlternative.Id);
            StatusMessage = $"{file.Name} was uploaded.";
        }
        catch (ApiException ex)
        {
            ValidationErrors = ex.Errors;
        }
        catch (IOException ex)
        {
            ValidationErrors = new() { $"The file could not be read: {ex.Message}" };
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Quick checks before calling the server. The server validates again.</summary>
    public static List<string> Validate(MaterialAlternative alternative)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(alternative.Name)) errors.Add("Name is required.");
        if (alternative.MaterialCategoryId <= 0) errors.Add("Choose a material category.");
        if (alternative.EnvironmentalData.Any(d => d.EmissionFactor < 0)) errors.Add("Emission factors cannot be negative.");
        if (alternative.Prices.Any(p => p.Amount < 0)) errors.Add("Prices cannot be negative.");
        return errors;
    }

    private void ResetMessages()
    {
        ValidationErrors = new();
        StatusMessage = null;
    }
}
