namespace Velux.Shared.Models;

public enum UserRole
{
    Procurement,
    Sustainability,
    SupplierRepresentative,
    Administrator
}

/// <summary>Primary = the supplier's own data (e.g. an EPD). Secondary = generic/industry data.</summary>
public enum SourceType
{
    Primary,
    Secondary
}

public enum DocumentType
{
    EPD,
    LCA,
    PCF,
    Other
}

public enum ComparisonOperator
{
    GreaterOrEqual,
    LessOrEqual,
    Equal
}

public enum AssessmentStatus
{
    Met,
    NotMet,
    Unknown
}
