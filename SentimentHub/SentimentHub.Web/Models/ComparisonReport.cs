using System.ComponentModel.DataAnnotations;

namespace SentimentHub.Web.Models;

public class ComparisonReport
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional Custom Name for easier identification
    public string? Name { get; set; }

    // Stored as JSON array string: "[1, 5, 8]"
    // Sorted IDs for consistent lookup
    [Required]
    public string AnalysisIds { get; set; } = string.Empty;

    // Serialized ComparisonViewModel
    [Required]
    public string ResultJson { get; set; } = string.Empty;
}
