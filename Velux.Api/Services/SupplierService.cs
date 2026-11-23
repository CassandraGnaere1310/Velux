using Microsoft.EntityFrameworkCore;
using Velux.Api.Data;
using Velux.Shared.Models;

namespace Velux.Api.Services;

/// <summary>Supplier and material data (FR1, FR2, FR10, UC1).</summary>
public class SupplierService(AppDbContext db) : ISupplierService
{
    public Task<List<Supplier>> GetSuppliersAsync() =>
        db.Suppliers.AsNoTracking()
            .Include(s => s.Alternatives)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public Task<List<MaterialCategory>> GetCategoriesAsync() =>
        db.MaterialCategories.AsNoTracking()
            .Include(c => c.Requirements)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public Task<List<MaterialAlternative>> GetAlternativesAsync(int categoryId) =>
        db.MaterialAlternatives.AsNoTracking()
            .Where(a => a.MaterialCategoryId == categoryId)
            .Include(a => a.Supplier)
            .Include(a => a.Specifications)
            .Include(a => a.EnvironmentalData)
            .Include(a => a.Prices)
            .OrderBy(a => a.Name)
            .ToListAsync();

    public Task<MaterialAlternative?> GetAlternativeAsync(int id) =>
        db.MaterialAlternatives.AsNoTracking()
            .Include(a => a.Supplier)
            .Include(a => a.Specifications)
            .Include(a => a.EnvironmentalData).ThenInclude(d => d.Documents)
            .Include(a => a.Prices)
            .FirstOrDefaultAsync(a => a.Id == id);

    /// <summary>UC1 exception 8a: required information is missing or invalid.</summary>
    public async Task<List<string>> ValidateAsync(MaterialAlternative a)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(a.Name)) errors.Add("Name is required.");
        if (!await db.Suppliers.AnyAsync(s => s.Id == a.SupplierId)) errors.Add("The supplier does not exist.");
        if (!await db.MaterialCategories.AnyAsync(c => c.Id == a.MaterialCategoryId)) errors.Add("Choose a material category.");

        if (a.Specifications.Any(s => string.IsNullOrWhiteSpace(s.Property)))
            errors.Add("Every technical specification needs a property name.");
        if (a.EnvironmentalData.Any(d => d.EmissionFactor < 0))
            errors.Add("Emission factors cannot be negative.");
        if (a.EnvironmentalData.Any(d => string.IsNullOrWhiteSpace(d.ReferenceUnit)))
            errors.Add("Emission data needs a reference unit (e.g. kg or m2).");
        if (a.Prices.Any(p => p.Amount < 0))
            errors.Add("Prices cannot be negative.");

        return errors;
    }

    public async Task<MaterialAlternative> CreateAlternativeAsync(MaterialAlternative input)
    {
        var entity = new MaterialAlternative
        {
            Name = input.Name.Trim(),
            Grade = input.Grade.Trim(),
            SupplierId = input.SupplierId,
            MaterialCategoryId = input.MaterialCategoryId
        };
        entity.Specifications.AddRange(input.Specifications.Select(CopySpecification));
        entity.EnvironmentalData.AddRange(input.EnvironmentalData.Select(CopyEnvironmentalData));
        entity.Prices.AddRange(input.Prices.Select(CopyPrice));

        db.MaterialAlternatives.Add(entity);
        await db.SaveChangesAsync();
        return (await GetAlternativeAsync(entity.Id))!;
    }

    /// <summary>
    /// Updates name/grade/category, updates or adds specifications and emission data,
    /// and only ADDS prices (existing prices are kept as price history).
    /// </summary>
    public async Task<MaterialAlternative?> UpdateAlternativeAsync(int id, MaterialAlternative input)
    {
        var entity = await db.MaterialAlternatives
            .Include(a => a.Specifications)
            .Include(a => a.EnvironmentalData)
            .Include(a => a.Prices)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (entity is null) return null;

        entity.Name = input.Name.Trim();
        entity.Grade = input.Grade.Trim();
        entity.SupplierId = input.SupplierId;
        entity.MaterialCategoryId = input.MaterialCategoryId;

        foreach (var s in input.Specifications)
        {
            var existing = s.Id == 0 ? null : entity.Specifications.FirstOrDefault(x => x.Id == s.Id);
            if (existing is null)
            {
                entity.Specifications.Add(CopySpecification(s));
            }
            else
            {
                existing.Property = s.Property;
                existing.Value = s.Value;
                existing.Unit = s.Unit;
            }
        }

        foreach (var d in input.EnvironmentalData)
        {
            var existing = d.Id == 0 ? null : entity.EnvironmentalData.FirstOrDefault(x => x.Id == d.Id);
            if (existing is null)
            {
                entity.EnvironmentalData.Add(CopyEnvironmentalData(d));
            }
            else
            {
                existing.EmissionFactor = d.EmissionFactor;
                existing.ReferenceUnit = d.ReferenceUnit;
                existing.SystemBoundary = d.SystemBoundary;
                existing.SourceType = d.SourceType;
                existing.SourceReference = d.SourceReference;
                existing.QualityNote = d.QualityNote;
            }
        }

        entity.Prices.AddRange(input.Prices.Where(p => p.Id == 0).Select(CopyPrice));

        await db.SaveChangesAsync();
        return await GetAlternativeAsync(id);
    }

    public async Task<EnvironmentalDocument?> AddDocumentAsync(int environmentalDataId, string title, DocumentType type, string filePath)
    {
        var data = await db.EnvironmentalDataRecords
            .Include(d => d.Documents)
            .FirstOrDefaultAsync(d => d.Id == environmentalDataId);
        if (data is null) return null;

        var document = new EnvironmentalDocument
        {
            Title = title,
            DocumentType = type,
            IssueDate = DateTime.Today,
            FilePath = filePath
        };
        data.Documents.Add(document);
        await db.SaveChangesAsync();
        return document;
    }

    private static TechnicalSpecification CopySpecification(TechnicalSpecification s) =>
        new() { Property = s.Property.Trim(), Value = s.Value, Unit = s.Unit.Trim() };

    private static EnvironmentalData CopyEnvironmentalData(EnvironmentalData d) => new()
    {
        EmissionFactor = d.EmissionFactor,
        ReferenceUnit = d.ReferenceUnit.Trim(),
        SystemBoundary = d.SystemBoundary,
        SourceType = d.SourceType,
        SourceReference = d.SourceReference,
        QualityNote = d.QualityNote
    };

    private static MaterialPrice CopyPrice(MaterialPrice p) => new()
    {
        Amount = p.Amount,
        Currency = p.Currency,
        PriceUnit = p.PriceUnit.Trim(),
        ValidFrom = p.ValidFrom
    };
}
