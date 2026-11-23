namespace Velux.Api.Services;

/// <summary>CO2e calculations (FR3, FR8). Pure functions, so they are easy to unit test.</summary>
public class CO2eCalculator
{
    /// <summary>Total kg CO2e = emission factor (kg CO2e per unit) × quantity.</summary>
    public double Total(double emissionFactor, double quantity) => emissionFactor * quantity;

    /// <summary>Absolute difference from the baseline. Negative = lower emissions than baseline.</summary>
    public double Diff(double baseline, double alternative) => alternative - baseline;

    /// <summary>Percentage difference from the baseline. Returns 0 if the baseline is 0.</summary>
    public double DiffPct(double baseline, double alternative) =>
        baseline == 0 ? 0 : (alternative - baseline) / baseline * 100;
}
