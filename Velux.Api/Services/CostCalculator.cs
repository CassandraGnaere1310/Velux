namespace Velux.Api.Services;

/// <summary>Cost calculations (FR6).</summary>
public class CostCalculator
{
    public decimal Total(decimal pricePerUnit, double quantity) => pricePerUnit * (decimal)quantity;

    /// <summary>Positive = more expensive than the baseline (a cost premium), negative = a saving.</summary>
    public decimal Diff(decimal baseline, decimal alternative) => alternative - baseline;
}
