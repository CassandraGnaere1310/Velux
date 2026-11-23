using CommunityToolkit.Mvvm.ComponentModel;
using Velux.Client.ApiClients;
using Velux.Shared.Models;

namespace Velux.Client.ViewModels;

/// <summary>ViewModel for SupplierComparisonPage (UC5 Compare Supplier Alternatives).</summary>
public partial class SupplierComparisonViewModel(
    IComparisonApiClient comparisonApi,
    ISupplierApiClient supplierApi) : ObservableObject
{
    [ObservableProperty] private List<MaterialCategory> categories = new();
    [ObservableProperty] private MaterialCategory? selectedCategory;
    [ObservableProperty] private List<MaterialAlternative> availableAlternatives = new();
    [ObservableProperty] private SupplierComparison currentComparison = new() { Name = "New comparison" };
    [ObservableProperty] private List<SupplierComparison> savedComparisons = new();
    [ObservableProperty] private List<string> validationErrors = new();
    [ObservableProperty] private string? statusMessage;
    [ObservableProperty] private bool isBusy;

    public bool HasResults =>
        CurrentComparison.Entries.Any(e => e.TotalCO2e.HasValue || e.TotalCost.HasValue || e.TechnicalAssessments.Count > 0);

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            Categories = await supplierApi.GetCategoriesAsync();
            SavedComparisons = await comparisonApi.GetAllAsync();
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

    public async Task LoadAlternativesAsync(int categoryId)
    {
        ResetMessages();
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == categoryId);
        AvailableAlternatives = categoryId > 0 ? await supplierApi.GetAlternativesAsync(categoryId) : new();
        CurrentComparison = new SupplierComparison { Name = "New comparison", MaterialCategoryId = categoryId };
    }

    public void AddAlternative(int alternativeId, double quantity, string unit)
    {
        var alternative = AvailableAlternatives.FirstOrDefault(a => a.Id == alternativeId);
        if (alternative is null) return;

        CurrentComparison.Entries.Add(new ComparisonEntry
        {
            MaterialAlternativeId = alternative.Id,
            MaterialAlternative = alternative,
            Quantity = quantity,
            QuantityUnit = unit,
            IsBaseline = CurrentComparison.Entries.Count == 0 // the first one added is the baseline by default
        });
        ClearResults();
    }

    public void RemoveEntry(ComparisonEntry entry)
    {
        CurrentComparison.Entries.Remove(entry);
        if (CurrentComparison.Entries.Count > 0 && !CurrentComparison.Entries.Any(e => e.IsBaseline))
            CurrentComparison.Entries[0].IsBaseline = true;
        ClearResults();
    }

    public void SelectBaseline(ComparisonEntry baseline)
    {
        foreach (var entry in CurrentComparison.Entries)
            entry.IsBaseline = entry == baseline;
        ClearResults();
    }

    public async Task CalculateAsync()
    {
        ResetMessages();
        ValidationErrors = Validate(CurrentComparison, forSaving: false);
        if (ValidationErrors.Count > 0) return;

        IsBusy = true;
        try
        {
            CurrentComparison = await comparisonApi.CalculateAsync(ToRequest(CurrentComparison));
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

    public async Task SaveAsync()
    {
        ResetMessages();
        ValidationErrors = Validate(CurrentComparison, forSaving: true);
        if (ValidationErrors.Count > 0) return;

        IsBusy = true;
        try
        {
            CurrentComparison = await comparisonApi.SaveAsync(ToRequest(CurrentComparison));
            SavedComparisons = await comparisonApi.GetAllAsync();
            StatusMessage = "The comparison was saved.";
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

    public async Task LoadAsync(int comparisonId)
    {
        ResetMessages();
        IsBusy = true;
        try
        {
            var comparison = await comparisonApi.GetAsync(comparisonId);
            if (comparison is null) return;

            SelectedCategory = Categories.FirstOrDefault(c => c.Id == comparison.MaterialCategoryId);
            AvailableAlternatives = await supplierApi.GetAlternativesAsync(comparison.MaterialCategoryId);
            CurrentComparison = comparison;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public static List<string> Validate(SupplierComparison comparison, bool forSaving)
    {
        var errors = new List<string>();
        if (forSaving && string.IsNullOrWhiteSpace(comparison.Name)) errors.Add("Give the comparison a name.");
        if (comparison.Entries.Count < 2) errors.Add("Add at least two alternatives to compare.");
        if (comparison.Entries.Any(e => e.Quantity <= 0)) errors.Add("Quantities must be greater than zero.");
        return errors;
    }

    /// <summary>Sends only the input fields to the server (not the loaded objects or old results).</summary>
    private static SupplierComparison ToRequest(SupplierComparison c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        MaterialCategoryId = c.MaterialCategoryId,
        CreatedByUserId = c.CreatedByUserId,
        Entries = c.Entries.Select(e => new ComparisonEntry
        {
            MaterialAlternativeId = e.MaterialAlternativeId,
            EnvironmentalDataId = e.EnvironmentalDataId,
            MaterialPriceId = e.MaterialPriceId,
            Quantity = e.Quantity,
            QuantityUnit = e.QuantityUnit,
            IsBaseline = e.IsBaseline
        }).ToList()
    };

    private void ClearResults()
    {
        foreach (var e in CurrentComparison.Entries)
        {
            e.TotalCO2e = null;
            e.TotalCost = null;
            e.CO2eDifference = null;
            e.CO2eDifferencePct = null;
            e.CostDifference = null;
            e.TechnicalAssessments = new();
        }
        OnPropertyChanged(nameof(CurrentComparison));
    }

    private void ResetMessages()
    {
        ValidationErrors = new();
        StatusMessage = null;
    }
}
