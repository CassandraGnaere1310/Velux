using System.Text.Json.Serialization;

namespace Velux.Shared.Models;

/// <summary>An uploaded EPD, LCA or PCF. The file is stored on disk; only metadata is stored in the database.</summary>
public class EnvironmentalDocument
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DocumentType DocumentType { get; set; }
    public DateTime IssueDate { get; set; }
    public string FilePath { get; set; } = "";

    [JsonIgnore]
    public List<EnvironmentalData> EnvironmentalData { get; set; } = new();
}
