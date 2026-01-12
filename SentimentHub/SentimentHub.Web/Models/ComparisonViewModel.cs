using SentimentHub.Web.DTOs;

namespace SentimentHub.Web.Models;

public class ComparisonViewModel
{
    public List<ComparedProduct> Products { get; set; } = new();

    // "Ayırt Edici Özellik Analizi"
    // Difference >= 1.0 in category scores
    public List<string> DistinctiveFeatures { get; set; } = new();

    // "Tercih Edilme Sebepleri"
    // Positive rate diff >= 15%
    public List<string> PreferenceReasons { get; set; } = new();

    // "Kullanıcı Tercih Profilleri"
    // Quality, Price/Perf, Speed -> Winner Product Name
    public Dictionary<string, string> UserProfiles { get; set; } = new();

    // "Karar Destek Sonucu"
    // 3-4 sentence summary
    public string DecisionSupport { get; set; } = string.Empty;
}

public class ComparedProduct
{
    public int AnalysisId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    
    // Formatted 1 decimal place (e.g. 4.3)
    public double OverallScore { get; set; }
    
    public double PositiveRate { get; set; }
    public double NegativeRate { get; set; }

    public CategoryScoresDto CategoryScores { get; set; } = new();
    
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    
    // "İşletme İçin İyileştirme Önerileri"
    // Generated from Weaknesses
    public List<string> Recommendations { get; set; } = new();
}
