using Velux.Api.Services;

namespace Velux.Tests;

public class CO2eCalculatorTests
{
    private readonly CO2eCalculator calculator = new();

    [Fact]
    public void Total_MultipliesFactorByQuantity()
    {
        Assert.Equal(8600.0, calculator.Total(emissionFactor: 8.6, quantity: 1000), 6);
    }

    [Fact]
    public void Diff_IsNegative_WhenAlternativeIsLower()
    {
        Assert.Equal(-4600.0, calculator.Diff(baseline: 8600, alternative: 4000), 6);
    }

    [Fact]
    public void DiffPct_ReturnsMinus25_WhenAlternativeIsQuarterLower()
    {
        Assert.Equal(-25.0, calculator.DiffPct(baseline: 100, alternative: 75), 6);
    }

    [Fact]
    public void DiffPct_ReturnsZero_WhenBaselineIsZero()
    {
        Assert.Equal(0.0, calculator.DiffPct(baseline: 0, alternative: 50), 6);
    }
}
