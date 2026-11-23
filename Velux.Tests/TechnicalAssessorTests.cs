using Velux.Api.Services;
using Velux.Shared.Models;

namespace Velux.Tests;

public class TechnicalAssessorTests
{
    private readonly TechnicalAssessor assessor = new();

    private static TechnicalRequirement MinimumStrength() => new()
    {
        Property = "Tensile strength",
        ComparisonOperator = ComparisonOperator.GreaterOrEqual,
        RequiredValue = 160,
        Unit = "MPa"
    };

    [Fact]
    public void Met_WhenValueMeetsRequirement()
    {
        var spec = new TechnicalSpecification { Property = "Tensile strength", Value = 205, Unit = "MPa" };
        Assert.Equal(AssessmentStatus.Met, assessor.Assess(MinimumStrength(), spec));
    }

    [Fact]
    public void NotMet_WhenValueIsTooLow()
    {
        var spec = new TechnicalSpecification { Property = "Tensile strength", Value = 150, Unit = "MPa" };
        Assert.Equal(AssessmentStatus.NotMet, assessor.Assess(MinimumStrength(), spec));
    }

    [Fact]
    public void Unknown_WhenSpecificationIsMissing()
    {
        Assert.Equal(AssessmentStatus.Unknown, assessor.Assess(MinimumStrength(), null));
    }

    [Fact]
    public void Unknown_WhenUnitsDiffer()
    {
        var spec = new TechnicalSpecification { Property = "Tensile strength", Value = 205, Unit = "ksi" };
        Assert.Equal(AssessmentStatus.Unknown, assessor.Assess(MinimumStrength(), spec));
    }
}
