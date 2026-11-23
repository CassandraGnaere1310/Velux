namespace Velux.Shared.Models;

/// <summary>Whether a comparison entry meets one technical requirement: Met, NotMet or Unknown.</summary>
public class TechnicalAssessment
{
    public int TechnicalRequirementId { get; set; }
    public TechnicalRequirement? TechnicalRequirement { get; set; }
    public AssessmentStatus Status { get; set; }
}
