using Velux.Shared.Models;

namespace Velux.Api.Services;

/// <summary>Checks a material specification against a technical requirement (FR9).</summary>
public class TechnicalAssessor
{
    public AssessmentStatus Assess(TechnicalRequirement requirement, TechnicalSpecification? specification)
    {
        if (specification is null)
            return AssessmentStatus.Unknown;

        if (!string.Equals(requirement.Unit.Trim(), specification.Unit.Trim(), StringComparison.OrdinalIgnoreCase))
            return AssessmentStatus.Unknown;

        bool met = requirement.ComparisonOperator switch
        {
            ComparisonOperator.GreaterOrEqual => specification.Value >= requirement.RequiredValue,
            ComparisonOperator.LessOrEqual => specification.Value <= requirement.RequiredValue,
            ComparisonOperator.Equal => Math.Abs(specification.Value - requirement.RequiredValue) < 1e-9,
            _ => false
        };

        return met ? AssessmentStatus.Met : AssessmentStatus.NotMet;
    }
}
