using System.ComponentModel.DataAnnotations;

namespace SentimentHub.Web.Models;

public class Review
{
    public int Id { get; set; }

    public string? AuthorName { get; set; }
    
    [Required]
    public string Text { get; set; } = string.Empty;
    
    // Original date text or parsed date
    public DateTime? ReviewDate { get; set; }
    
    // Star rating given by user (1-5)
    public int Rating { get; set; }

    // Detected Sentiment
    public SentimentType Sentiment { get; set; }
    public double ConfidenceScore { get; set; }

    // Foreign Key
    public int AnalysisId { get; set; }
    public Analysis? Analysis { get; set; }

    // Navigation
    public ICollection<AspectResult> AspectResults { get; set; } = new List<AspectResult>();
}

public class AspectResult
{
    public int Id { get; set; }

    public AspectType Aspect { get; set; }
    public SentimentType Sentiment { get; set; }
    public double Confidence { get; set; }

    // Foreign Key
    public int ReviewId { get; set; }
    public Review? Review { get; set; }
}
