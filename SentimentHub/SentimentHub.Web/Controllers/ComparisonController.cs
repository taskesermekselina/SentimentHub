using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentimentHub.Web.Data;
using SentimentHub.Web.DTOs;
using SentimentHub.Web.Models;
using SentimentHub.Web.Services;
using System.Text.Json;

namespace SentimentHub.Web.Controllers;

[Authorize]
public class ComparisonController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPdfService _pdfService;

    public ComparisonController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IPdfService pdfService)
    {
        _context = context;
        _userManager = userManager;
        _pdfService = pdfService;
    }

    // LIST VIEW
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // 1. Get User's Analyses
        var analyses = await _context.Analyses
            .Include(a => a.Business)
            .Where(a => a.Business != null && a.Business.OwnerId == user.Id && a.Status == AnalysisStatus.Completed)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        // 2. Get User's Comparison History (Simplified: 50 recent)
        var recentComparisons = await _context.ComparisonReports
            .OrderByDescending(c => c.CreatedAt)
            .Take(50)
            .ToListAsync();

        var userAnalysisIds = new HashSet<int>(analyses.Select(a => a.Id));
        var userHistory = new List<ComparisonReport>();

        foreach(var comp in recentComparisons)
        {
            try {
                var ids = JsonSerializer.Deserialize<int[]>(comp.AnalysisIds);
                if(ids != null && ids.Any(id => userAnalysisIds.Contains(id)))
                {
                    userHistory.Add(comp);
                }
            } catch {}
        }

        ViewBag.History = userHistory;

        return View(analyses);
    }

    [HttpGet]
    public async Task<IActionResult> Load(int id)
    {
        var report = await _context.ComparisonReports.FindAsync(id);
        if(report == null) return NotFound();

        try 
        {
            var model = JsonSerializer.Deserialize<ComparisonViewModel>(report.ResultJson);
            if(model != null) return View("Result", model);
        }
        catch { }
        return RedirectToAction(nameof(Index));
    }

    // COMPARISON RESULT
    [HttpPost]
    public async Task<IActionResult> Result(int[] analysisIds, string? reportName)
    {
        if (analysisIds == null || analysisIds.Length < 2 || analysisIds.Length > 3)
        {
            TempData["Error"] = "Lütfen en az 2, en fazla 3 rapor seçiniz.";
            return RedirectToAction(nameof(Index));
        }

        // Sort IDs to ensure consistent key for storage
        Array.Sort(analysisIds);
        var idsKey = JsonSerializer.Serialize(analysisIds);

        // 1. Check if comparison exists
        var existingReport = await _context.ComparisonReports
            .FirstOrDefaultAsync(c => c.AnalysisIds == idsKey);

        if (existingReport != null)
        {
            // Update Name if provided and different
            if (!string.IsNullOrEmpty(reportName) && existingReport.Name != reportName)
            {
                existingReport.Name = reportName;
                await _context.SaveChangesAsync();
            }

            try 
            {
                var cachedModel = JsonSerializer.Deserialize<ComparisonViewModel>(existingReport.ResultJson);
                if (cachedModel != null)
                {
                    return View(cachedModel);
                }
            }
            catch { /* Fallback to regenerate if corrupt */ }
        }

        // 2. Generate New Comparison
        var viewModel = await PrepareComparisonModel(analysisIds);
        if (viewModel == null) return RedirectToAction(nameof(Index));

        // 3. Save Comparison Report
        var newReport = new ComparisonReport
        {
             AnalysisIds = idsKey,
             CreatedAt = DateTime.UtcNow,
             Name = reportName,
             ResultJson = JsonSerializer.Serialize(viewModel)
        };
        _context.ComparisonReports.Add(newReport);
        await _context.SaveChangesAsync();

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> DownloadPdf([FromBody] int[] analysisIds)
    {
        var viewModel = await PrepareComparisonModel(analysisIds);
        if (viewModel == null) return NotFound();

        var pdfBytes = _pdfService.GenerateComparisonReport(viewModel);
        return File(pdfBytes, "application/pdf", $"KarsilastirmaRaporu_{DateTime.Now:yyyyMMdd}.pdf");
    }

    private async Task<ComparisonViewModel> PrepareComparisonModel(int[] analysisIds)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;

        var analyses = await _context.Analyses
            .Include(a => a.Business)
            .Include(a => a.Reviews)
            .ThenInclude(r => r.AspectResults)
            .Where(a => analysisIds.Contains(a.Id) && a.Business.OwnerId == user.Id)
            .ToListAsync();

        if (analyses.Count < 2) return null;

        var model = new ComparisonViewModel();

        foreach (var analysis in analyses)
        {
            var summary = JsonSerializer.Deserialize<SummaryResultDto>(analysis.SummaryJson ?? "{}");
            if (summary == null) continue;

            var product = new ComparedProduct
            {
                AnalysisId = analysis.Id,
                Name = analysis.Business?.Name ?? "Bilinmeyen Ürün",
                Url = analysis.Business?.GoogleMapsUrl ?? "",
                CategoryScores = summary.CategoryScores ?? new CategoryScoresDto(),
                Strengths = summary.Strengths ?? new List<string>(),
                Weaknesses = summary.Weaknesses ?? new List<string>()
            };

             // 1. Calculate Score (Weighted Avg) - 1 decimal
            double avgScore = 0;
            if (summary.CategoryScores != null)
            {
                var scores = new[] { 
                    summary.CategoryScores.ProductQuality, 
                    summary.CategoryScores.PricePerformance, 
                    summary.CategoryScores.Shipping, 
                    summary.CategoryScores.Seller, 
                    summary.CategoryScores.UsageExperience 
                };
                avgScore = scores.Average();
            }
            product.OverallScore = Math.Round(avgScore, 1);

            // 2. Positive/Negative Rates
            if (analysis.Reviews != null && analysis.Reviews.Any())
            {
                double total = analysis.Reviews.Count;
                double pos = analysis.Reviews.Count(r => r.Sentiment == SentimentType.Positive);
                double neg = analysis.Reviews.Count(r => r.Sentiment == SentimentType.Negative);
                
                product.PositiveRate = Math.Round((pos / total) * 100, 1);
                product.NegativeRate = Math.Round((neg / total) * 100, 1);
            }

            // 3. Generate Recommendations (Using Shared Service)
            product.Recommendations = RecommendationEngine.Generate(product.Weaknesses);

            model.Products.Add(product);
        }

        // 4. Analysis Logic (Distinctive, Profiles, etc.)
        GenerateComparisonInsights(model);

        return model;
    }

    // Removed private GenerateRecommendations since we use RecommendationEngine now

    private void GenerateComparisonInsights(ComparisonViewModel model)
    {
        // A) Distinctive Features (Score Diff >= 1.0)
        // Check pairs. If we have 3 products, A-B, B-C, A-C? Or just find significant outliers?
        // Prompt says: "Ürün A, [Cat] alanında Ürün B'ye kıyasla..."
        // Ensure we cover main comparisons.
        
        var products = model.Products;
        for (int i = 0; i < products.Count; i++)
        {
            for (int j = i + 1; j < products.Count; j++)
            {
                var p1 = products[i];
                var p2 = products[j];

                CompareCategories(p1, p2, model.DistinctiveFeatures);
                CompareCategories(p2, p1, model.DistinctiveFeatures);

                // B) Preference Reasons (Pos Rate Diff >= 15%)
                double diff = p1.PositiveRate - p2.PositiveRate;
                if (diff >= 15.0)
                {
                    model.PreferenceReasons.Add($"{p1.Name}, {p2.Name} ürününe göre %{diff:F1} daha yüksek müşteri memnuniyet oranına sahip olduğu için güçlü bir tercih sebebidir.");
                }
                else if (diff <= -15.0)
                {
                    model.PreferenceReasons.Add($"{p2.Name}, {p1.Name} ürününe göre %{Math.Abs(diff):F1} daha yüksek müşteri memnuniyet oranına sahip olduğu için güçlü bir tercih sebebidir.");
                }
            }
        }

        // C) User Profiles
        // Quality: Max ProductQuality
        var qualityWinner = products.OrderByDescending(p => p.CategoryScores.ProductQuality).First();
        model.UserProfiles["Kalite Odaklı Kullanıcı"] = qualityWinner.Name;

        // Price/Perf: Max PricePerformance
        var priceWinner = products.OrderByDescending(p => p.CategoryScores.PricePerformance).First();
        model.UserProfiles["Fiyat/Performans Odaklı Kullanıcı"] = priceWinner.Name;

        // Speed: Max Shipping
        var speedWinner = products.OrderByDescending(p => p.CategoryScores.Shipping).First();
        model.UserProfiles["Hız ve Teslimat Odaklı Kullanıcı"] = speedWinner.Name;

        // D) Decision Support
        // Summary of who won what.
        model.DecisionSupport = $"Genel değerlendirmede {products.OrderByDescending(p => p.OverallScore).First().Name} en yüksek puanı alarak ({products.Max(p => p.OverallScore)}) öne çıkmaktadır. ";
        if (qualityWinner == priceWinner)
        {
            model.DecisionSupport += $"Hem kalite hem de fiyat/performans arayan kullanıcılar için {qualityWinner.Name} ideal bir seçenektir.";
        }
        else
        {
            model.DecisionSupport += $"Kalite önceliğinde {qualityWinner.Name}, bütçe dostu kullanımda ise {priceWinner.Name} tavsiye edilir.";
        }
    }

    private void CompareCategories(ComparedProduct main, ComparedProduct other, List<string> features)
    {
        var cats = new List<string>();
        if (main.CategoryScores.ProductQuality - other.CategoryScores.ProductQuality >= 1.0) cats.Add("Ürün Kalitesi");
        if (main.CategoryScores.PricePerformance - other.CategoryScores.PricePerformance >= 1.0) cats.Add("Fiyat/Performans");
        if (main.CategoryScores.Shipping - other.CategoryScores.Shipping >= 1.0) cats.Add("Kargo Hızı");
        if (main.CategoryScores.Seller - other.CategoryScores.Seller >= 1.0) cats.Add("Satıcı İlgisi");
        if (main.CategoryScores.UsageExperience - other.CategoryScores.UsageExperience >= 1.0) cats.Add("Kullanım Deneyimi");

        if (cats.Any())
        {
            string catStr = string.Join(" ve ", cats); // Simple join, "A, B ve C" logic bit complex for now, just " ve " safe enough or comma
            features.Add($"{main.Name}, [{catStr}] alanlarında {other.Name}'ye kıyasla belirgin bir üstünlük göstermektedir.");
        }
    }
}
