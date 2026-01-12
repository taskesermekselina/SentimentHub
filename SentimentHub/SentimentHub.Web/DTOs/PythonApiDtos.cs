using System.Text.Json.Serialization;

namespace SentimentHub.Web.DTOs;

public class AnalyzeRequestDto
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("limit")]
    public int Limit { get; set; } = 50;
}

public class AspectResultDto
{
    [JsonPropertyName("aspect")]
    public string Aspect { get; set; } = string.Empty;

    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}

public class ReviewResultDto
{
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = string.Empty;

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("aspects")]
    public List<AspectResultDto> Aspects { get; set; } = new();
}

public class CategoryScoresDto
{
    [JsonPropertyName("productQuality")]
    public double ProductQuality { get; set; }

    [JsonPropertyName("pricePerformance")]
    public double PricePerformance { get; set; }

    [JsonPropertyName("shipping")]
    public double Shipping { get; set; }

    [JsonPropertyName("seller")]
    public double Seller { get; set; }

    [JsonPropertyName("usageExperience")]
    public double UsageExperience { get; set; }
}

public class SummaryResultDto
{
    [JsonPropertyName("overallScore")]
    public double OverallScore { get; set; }

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("weaknesses")]
    public List<string> Weaknesses { get; set; } = new();

    [JsonPropertyName("recommendations")]
    public List<string> Recommendations { get; set; } = new();

    [JsonPropertyName("categoryScores")]
    public CategoryScoresDto CategoryScores { get; set; } = new();
}

public class AnalyzeResponseDto
{
    [JsonPropertyName("reviews")]
    public List<ReviewResultDto> Reviews { get; set; } = new();

    [JsonPropertyName("total_reviews")]
    public int TotalReviews { get; set; }

    [JsonPropertyName("overall_sentiment_score")]
    public double OverallSentimentScore { get; set; }

    [JsonPropertyName("business_name")]
    public string? BusinessName { get; set; }

    [JsonPropertyName("summary")]
    public SummaryResultDto? Summary { get; set; }
}
