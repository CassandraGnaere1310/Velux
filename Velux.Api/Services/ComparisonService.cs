using Microsoft.EntityFrameworkCore;
using Velux.Api.Data;
using Velux.Shared.Models;

namespace Velux.Api.Services;

/// <summary>Compares supplier alternatives (FR3, FR4, FR6, FR8, FR9, FR10).</summary>
public class ComparisonService(
    AppDbContext db,
    CO2eCalculator co2,
    CostCalculator cost,
    TechnicalAssessor assessor) : IComparisonService
{
    public Task<List<SupplierComparison>> GetAllAsync() =>
        db.SupplierComparisons.AsNoTracking()
            .Include(c => c.MaterialCategory)
            .Include(c => c.Entries)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    /// <summary>Loads a saved comparison and recalculates it with the exact data it was saved with.</summary>
    public async Task<SupplierComparison?> GetAsync(int id)
    {
        var comparison = await db.SupplierComparisons.AsNoTracking()
            .Include(c => c.MaterialCategory)
            .Include(c => c.Entries)
            .FirstOrDefaultAsync(c => c.Id == id);

        return comparison is null ? null : await CalculateAsync(comparison);
    }

    public async Task<List<string>> ValidateAsync(SupplierComparison c, bool forSaving)
    {
        var errors = new List<string>();

        if (forSaving && string.IsNullOrWhiteSpace(c.Name)) errors.Add("The comparison needs a name.");
        if (c.Entries.Count < 2) errors.Add("Add at least two alternatives to compare.");
        if (c.Entries.Count(e => e.IsBaseline) > 1) errors.Add("Only one entry can be the baseline.");
        if (c.Entries.Any(e => e.Quantity <= 0)) errors.Add("Quantities must be greater than zero.");

        var ids = c.Entries.Select(e => e.MaterialAlternativeId).Distinct().ToList();
        var alternatives = await db.MaterialAlternatives.AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .Select(a => new { a.Id, a.MaterialCategoryId })
            .ToListAsync();

        if (alternatives.Count != ids.Count)
            errors.Add("One or more alternatives do not exist.");

        // Constraint from the domain model: all alternatives must belong to the comparison's category
        if (alternatives.Any(a => a.MaterialCategoryId != c.MaterialCategoryId))
            errors.Add("All alternatives must belong to the comparison's material category.");

        return errors;
    }

    public async Task<SupplierComparison> CalculateAsync(SupplierComparison c)
    {
        var ids = c.Entries.Select(e => e.MaterialAlternativeId).Distinct().ToList();

        var alternatives = await db.MaterialAlternatives.AsNoTracking()
            .Where(a => ids.Contains(a.Id))
            .Include(a => a.Supplier)
            .Include(a => a.Specifications)
            .Include(a => a.EnvironmentalData).ThenInclude(d => d.Documents)
            .Include(a => a.Prices)
            .ToDictionaryAsync(a => a.Id);

        var requirements = await db.TechnicalRequirements.AsNoTracking()
            .Where(r => r.MaterialCategoryId == c.MaterialCategoryId)
            .ToListAsync();

        foreach (var entry in c.Entries)
        {
            ClearResults(entry);
            if (!alternatives.TryGetValue(entry.MaterialAlternativeId, out var alternative)) continue;
            entry.MaterialAlternative = alternative;

            // Which data does this entry use? Keep the stored choice; otherwise prefer primary data, then latest price.
            // Constraint: the data must belong to the alternative the entry evaluates.
            var environmentalData = alternative.EnvironmentalData.FirstOrDefault(d => d.Id == entry.EnvironmentalDataId)
                                    ?? alternative.EnvironmentalData.OrderBy(d => d.SourceType).FirstOrDefault();
            var price = alternative.Prices.FirstOrDefault(p => p.Id == entry.MaterialPriceId)
                        ?? alternative.Prices.OrderByDescending(p => p.ValidFrom).FirstOrDefault();

            entry.EnvironmentalData = environmentalData;
            entry.EnvironmentalDataId = environmentalData?.Id;
            entry.MaterialPrice = price;
            entry.MaterialPriceId = price?.Id;

            // Only calculate when the units match (e.g. factor per kg and quantity in kg)
            if (environmentalData is not null && SameUnit(environmentalData.ReferenceUnit, entry.QuantityUnit))
                entry.TotalCO2e = co2.Total(environmentalData.EmissionFactor, entry.Quantity);

            if (price is not null && SameUnit(price.PriceUnit, entry.QuantityUnit))
                entry.TotalCost = cost.Total(price.Amount, entry.Quantity);

            entry.TechnicalAssessments = requirements
                .Select(r => new TechnicalAssessment
                {
                    TechnicalRequirementId = r.Id,
                    TechnicalRequirement = r,
                    Status = assessor.Assess(r, alternative.Specifications.FirstOrDefault(s =>
                        string.Equals(s.Property.Trim(), r.Property.Trim(), StringComparison.OrdinalIgnoreCase)))
                })
                .ToList();
        }

        var baseline = c.Entries.FirstOrDefault(e => e.IsBaseline) ?? c.Entries.FirstOrDefault();
        if (baseline is not null)
        {
            baseline.IsBaseline = true;
            foreach (var entry in c.Entries)
            {
                if (entry.TotalCO2e is double alternativeCo2 && baseline.TotalCO2e is double baselineCo2)
                {
                    entry.CO2eDifference = co2.Diff(baselineCo2, alternativeCo2);
                    entry.CO2eDifferencePct = co2.DiffPct(baselineCo2, alternativeCo2);
                }

                if (entry.TotalCost is decimal alternativeCost && baseline.TotalCost is decimal baselineCost)
                    entry.CostDifference = cost.Diff(baselineCost, alternativeCost);
            }
        }

        return c;
    }

    public async Task<SupplierComparison> SaveAsync(SupplierComparison input)
    {
        // Calculate first, so each entry records exactly which emission data and price it used
        await CalculateAsync(input);

        var entity = new SupplierComparison
        {
            Name = input.Name.Trim(),
            CreatedAt = DateTime.UtcNow,
            MaterialCategoryId = input.MaterialCategoryId,
            CreatedByUserId = input.CreatedByUserId
        };
        entity.Entries.AddRange(input.Entries.Select(e => new ComparisonEntry
        {
            MaterialAlternativeId = e.MaterialAlternativeId,
            EnvironmentalDataId = e.EnvironmentalDataId,
            MaterialPriceId = e.MaterialPriceId,
            Quantity = e.Quantity,
            QuantityUnit = e.QuantityUnit,
            IsBaseline = e.IsBaseline
        }));

        db.SupplierComparisons.Add(entity);
        await db.SaveChangesAsync();
        return (await GetAsync(entity.Id))!;
    }

    private static bool SameUnit(string a, string b) =>
        string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    private static void ClearResults(ComparisonEntry e)
    {
        e.TotalCO2e = null;
        e.TotalCost = null;
        e.CO2eDifference = null;
        e.CO2eDifferencePct = null;
        e.CostDifference = null;
        e.TechnicalAssessments = new();
    }
}
