using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SentimentHub.Web.Models;

namespace SentimentHub.Web.Services;

public interface ILmStudioService
{
    Task<LmAnalysisResult> AnalyzeReviewAsync(string reviewText);
    Task<SentimentHub.Web.DTOs.SummaryResultDto> GenerateSummaryAsync(List<string> reviews);
}

public class LmStudioService : ILmStudioService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LmStudioService> _logger;

    public LmStudioService(HttpClient httpClient, ILogger<LmStudioService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("http://localhost:1234/v1/");
        _httpClient.Timeout = TimeSpan.FromMinutes(2); // Local LLMs can be slow
    }

    public async Task<LmAnalysisResult> AnalyzeReviewAsync(string reviewText)
    {
        var prompt = @"
Sen bir e-ticaret ürün yorumlarını analiz eden yapay zekasın.
Aşağıdaki yorumu analiz et ve sonucu SADECE JSON formatında ver.
Başka hiçbir ek açıklama yapma.

KATEGORİLER (Sadece bunlardan birini seç):
1. ProductQuality (Ürün Kalitesi)
2. PricePerformance (Fiyat / Performans)
3. Delivery (Kargo ve Teslimat)
4. PackagingSeller (Satıcı / Paketleme)
5. GeneralSatisfaction (Genel Memnuniyet - Hiçbirine uymuyorsa veya genel ise)

Kurallar:
- sentiment: 'Positive', 'Negative' veya 'Neutral' olmalı.
- score: 1 ile 5 arasında tam sayı olmalı.
- category: Yukarıdaki İngilizce kategori ismini (örn: 'ProductQuality') kullan.

İstenen JSON Formatı:
{
  ""category"": ""ProductQuality"",
  ""sentiment"": ""Positive"",
  ""score"": 5
}

Yorum: """ + reviewText + @"""";

        var requestBody = new
        {
            messages = new[]
            {
                new { role = "system", content = "Sen yardımcı bir asistansın. Her zaman sadece JSON çıktısı ver." },
                new { role = "user", content = prompt }
            },
            temperature = 0.1, // Deterministic results
            max_tokens = 100,
            stream = false
        };

        try
        {
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"LM Studio Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return GetDefaultResult();
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var contentText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // Clean up Markdown if present (```json ... ```)
            if (contentText.Contains("```"))
            {
                contentText = contentText.Replace("```json", "").Replace("```", "").Trim();
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<LmAnalysisResult>(contentText, options) ?? GetDefaultResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LM Studio call failed");
            return GetDefaultResult();
        }
    }

    private LmAnalysisResult GetDefaultResult()
    {
        return new LmAnalysisResult
        {
            Category = "GeneralSatisfaction",
            Sentiment = "Neutral",
            Score = 3
        };
    }

    public async Task<SentimentHub.Web.DTOs.SummaryResultDto> GenerateSummaryAsync(List<string> reviews)
    {
        // Limit reviews to avoid token limits (e.g. first 20 + last 20 or random)
        var sampleReviews = reviews.Count > 50 ? reviews.Take(50).ToList() : reviews;
        var reviewsJson = JsonSerializer.Serialize(sampleReviews);

        var prompt = @"
Sen uzman bir ürün inceleme analistisin. Aşağıdaki müşteri yorumlarını analiz ederek bir özet rapor hazırla.

DEĞERLENDİRME SİSTEMİ KURALLARI (ZORUNLU):
1. 'overallScore' MUTLAKA 1.0 ile 5.0 arasında (ondalıklı olabilir, örn: 4.3) bir sayı olmalı.
2. Kategori Puanları (categoryScores) MUTLAKA 1.0 ile 5.0 arasında ONDALIKLI (örn: 3.8, 4.2) olmalı.
3. Puanlama Mantığı:
   - Pozitif yorum oranı yüksekse -> 4.0 - 5.0 arası
   - Karışık/Dengeli yorumlar -> 2.5 - 3.9 arası
   - Negatif yorum oranı yüksekse -> 1.0 - 2.4 arası

KATEGORİLER:
- productQuality: Ürün Kalitesi
- pricePerformance: Fiyat / Performans
- shipping: Kargo ve Paketleme
- seller: Satıcı İletişimi
- usageExperience: Kullanım Deneyimi

ÇIKTI FORMATI (SADECE JSON):
{
  ""overallScore"": 4.3,
  ""strengths"": [""Güçlü yön 1"", ""Güçlü yön 2"", ...],
  ""weaknesses"": [""Zayıf yön 1"", ""Zayıf yön 2"", ...],
  ""categoryScores"": {
    ""productQuality"": 4.2,
    ""pricePerformance"": 4.8,
    ""shipping"": 3.5,
    ""seller"": 4.0,
    ""usageExperience"": 4.6
  }
}

Yorumlar:
" + reviewsJson;

        var requestBody = new
        {
            messages = new[]
            {
                new { role = "system", content = "Sen JSON formatında çıktı veren bir yapay zeka asistanısın. Sadece JSON döndür." },
                new { role = "user", content = prompt }
            },
            temperature = 0.2, 
            max_tokens = 800,
            stream = false
        };

        try
        {
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"LM Studio Summary Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return GetDefaultSummary();
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var contentText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (contentText.Contains("```"))
            {
                contentText = contentText.Replace("```json", "").Replace("```", "").Trim();
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<SentimentHub.Web.DTOs.SummaryResultDto>(contentText, options) ?? GetDefaultSummary();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LM Studio Summary call failed");
            return GetDefaultSummary();
        }
    }

    private SentimentHub.Web.DTOs.SummaryResultDto GetDefaultSummary()
    {
        return new SentimentHub.Web.DTOs.SummaryResultDto
        {
            OverallScore = 3.0,
            Strengths = new List<string> { "Veri analizi yapılamadı." },
            Weaknesses = new List<string>(),
            CategoryScores = new SentimentHub.Web.DTOs.CategoryScoresDto { ProductQuality = 3, PricePerformance = 3, Seller = 3, Shipping = 3, UsageExperience = 3 }
        };
    }
}


public class LmAnalysisResult
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = "GeneralSatisfaction";

    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = "Neutral";

    [JsonPropertyName("score")]
    public int Score { get; set; } = 3;
}
