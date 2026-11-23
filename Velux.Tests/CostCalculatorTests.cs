using Velux.Api.Services;

namespace Velux.Tests;

public class CostCalculatorTests
{
    private readonly CostCalculator calculator = new();

    [Fact]
    public void Total_MultipliesPriceByQuantity()
    {
        Assert.Equal(3100m, calculator.Total(pricePerUnit: 3.10m, quantity: 1000));
    }

    [Fact]
    public void Diff_IsPositive_WhenAlternativeCostsMore()
    {
        Assert.Equal(350m, calculator.Diff(baseline: 3100m, alternative: 3450m));
    }
}
