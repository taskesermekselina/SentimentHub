namespace SentimentHub.Web.Models;

public enum AnalysisStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public enum SentimentType
{
    Positive,
    Negative,
    Neutral
}

public enum AspectType
{
    ProductQuality,      // Ürün Kalitesi
    PricePerformance,    // Fiyat / Performans
    Delivery,            // Kargo ve Teslimat
    PackagingSeller,     // Satıcı / Paketleme
    GeneralSatisfaction  // Genel Memnuniyet
}
