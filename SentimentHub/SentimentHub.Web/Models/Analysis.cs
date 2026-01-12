using System.ComponentModel.DataAnnotations;

namespace SentimentHub.Web.Models;

public class Analysis
{
    public int Id { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;

    public int TotalReviews { get; set; }
    
    // Overall sentiment score (e.g. 0.75 for 75% positive/confidence weighted)
    public double OverallScore { get; set; }
    
    public string? ErrorMessage { get; set; }

    // Stores the AI-generated strategic summary (JSON)
    public string? SummaryJson { get; set; }

    // Foreign Key
    public int BusinessId { get; set; }
    public Business? Business { get; set; }

    // Navigation
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
