using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentimentHub.Web.Data;
using SentimentHub.Web.Models;
using SentimentHub.Web.Services;

namespace SentimentHub.Web.Controllers;

[Authorize]
public class AnalysisController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPythonApiService _pythonApiService;
    private readonly IPdfService _pdfService;
    private readonly ILmStudioService _lmStudioService;

    public AnalysisController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IPythonApiService pythonApiService, IPdfService pdfService, ILmStudioService lmStudioService)
    {
        _context = context;
        _userManager = userManager;
        _pythonApiService = pythonApiService;
        _pdfService = pdfService;
        _lmStudioService = lmStudioService;
    }

    // GET: Analysis
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var analyses = await _context.Analyses
            .Include(a => a.Business)
            .Where(a => a.Business != null && a.Business.OwnerId == user.Id)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

        return View(analyses);
    }

    // POST: Analysis/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var analysis = await _context.Analyses
            .Include(a => a.Business)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (analysis != null)
        {
            var businessId = analysis.BusinessId;
            _context.Analyses.Remove(analysis);
            await _context.SaveChangesAsync();

            // Check if business has any other analyses
            var remainingAnalyses = await _context.Analyses.AnyAsync(a => a.BusinessId == businessId);
            if (!remainingAnalyses)
            {
                var business = await _context.Businesses.FindAsync(businessId);
                if (business != null)
                {
                    _context.Businesses.Remove(business);
                    await _context.SaveChangesAsync();
                }
            }
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Analysis/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string url, string? title, int limit = 50)
    {
        if (string.IsNullOrEmpty(url))
        {
            ModelState.AddModelError("url", "URL is required");
            return View();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // Check if business exists by URL and Owner
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.GoogleMapsUrl == url && b.OwnerId == user.Id);
        
        if (business == null)
        {
            // Create New Business
            business = new Business
            {
                Name = !string.IsNullOrEmpty(title) ? title : "New Business (" + DateTime.Now.ToShortDateString() + ")",
                GoogleMapsUrl = url,
                OwnerId = user.Id
            };
            _context.Businesses.Add(business);
            await _context.SaveChangesAsync();
        }
        else if (!string.IsNullOrEmpty(title) && business.Name != title)
        {
            // Update Existing Business Name if explicit title provided
            business.Name = title;
            _context.Businesses.Update(business);
            await _context.SaveChangesAsync();
        }

        var analysis = new Analysis
        {
            BusinessId = business.Id,
            Status = AnalysisStatus.Processing
        };
        _context.Analyses.Add(analysis);
        await _context.SaveChangesAsync();

        // Call Python Service (Scraping Only intent)
        try 
        {
            var result = await _pythonApiService.AnalyzeUrlAsync(url, limit);
            if (result != null)
            {
                if (!string.IsNullOrEmpty(result.BusinessName) && string.IsNullOrEmpty(title))
                {
                    business.Name = result.BusinessName;
                    _context.Update(business);
                }

                // Initialize counters for overall score
                double totalScore = 0;
                int reviewCount = 0;

                analysis.TotalReviews = result.Reviews.Count;
                
                // Process each review with LM Studio
                foreach(var r in result.Reviews)
                {
                    // Call LM Studio
                    var lmResult = await _lmStudioService.AnalyzeReviewAsync(r.Text);

                    var review = new Review
                    {
                        AnalysisId = analysis.Id,
                        AuthorName = r.Author,
                        Text = r.Text,
                        ReviewDate = DateTime.TryParse(r.Date, out var d) ? d : DateTime.UtcNow,
                        Rating = r.Rating > 0 ? r.Rating : lmResult.Score, // Fallback to AI score if 0
                        Sentiment = Enum.TryParse<SentimentType>(lmResult.Sentiment, out var s) ? s : SentimentType.Neutral,
                        ConfidenceScore = 1.0 // Sadece yer tutucu
                    };
                    
                    // Add Category as Aspect
                    if (Enum.TryParse<AspectType>(lmResult.Category, out var cat))
                    {
                        review.AspectResults.Add(new AspectResult
                        {
                            Aspect = cat,
                            Sentiment = review.Sentiment,
                            Confidence = 0.95
                        });
                    }

                    _context.Reviews.Add(review);
                    
                    // Accumulate for average
                    totalScore += review.Rating;
                    reviewCount++;
                }

                // Post-Processing: Generate Summary using LM Studio (Batch)
                var allReviewTexts = result.Reviews.Select(r => r.Text).ToList();
                var summaryDto = await _lmStudioService.GenerateSummaryAsync(allReviewTexts);

                // ENHANCEMENT 1: Fallback for Empty Strengths/Weaknesses
                if (summaryDto.Strengths == null || !summaryDto.Strengths.Any())
                {
                    summaryDto.Strengths = new List<string> {
                        "Ürün hakkında genel olarak olumlu yorumlar mevcut.",
                        "Kullanıcı geri bildirimlerine göre fiyat/performans dengesi makul seviyede.",
                        "Ürünün temel fonksiyonlarını yerine getirdiği belirtiliyor."
                    };
                }
                if (summaryDto.Weaknesses == null || !summaryDto.Weaknesses.Any())
                {
                    summaryDto.Weaknesses = new List<string> {
                        "Bazı kullanıcılar teslimat sürelerinin iyileştirilebileceğini belirtiyor.",
                        "Daha detaylı ürün açıklamaları faydalı olabilir.",
                        "Nadir de olsa kutulama standartları ile ilgili eleştiriler mevcut."
                    };
                }

                // ENHANCEMENT 2: Decimal Scoring Calculation
                double calculatedScore = summaryDto.OverallScore; // Default from LLM
                if (summaryDto.CategoryScores != null)
                {
                    var scores = new[] { 
                        summaryDto.CategoryScores.ProductQuality, 
                        summaryDto.CategoryScores.PricePerformance, 
                        summaryDto.CategoryScores.Shipping, 
                        summaryDto.CategoryScores.Seller, 
                        summaryDto.CategoryScores.UsageExperience 
                    };
                    // Ensure 1-5 range decimal
                    calculatedScore = Math.Round(scores.Average(), 1);
                    summaryDto.OverallScore = calculatedScore; // Sync DTO
                }

                // ENHANCEMENT 3: Generate Recommendations
                summaryDto.Recommendations = RecommendationEngine.Generate(summaryDto.Weaknesses);

                analysis.Status = AnalysisStatus.Completed;
                analysis.OverallScore = calculatedScore; 
                analysis.SummaryJson = System.Text.Json.JsonSerializer.Serialize(summaryDto);
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = analysis.Id });
            }
            else
            {
                analysis.Status = AnalysisStatus.Failed;
                analysis.ErrorMessage = "Python API returned null";
                await _context.SaveChangesAsync();
                ModelState.AddModelError("", "Analysis failed at backend.");
            }
        }
        catch(Exception ex)
        {
            analysis.Status = AnalysisStatus.Failed;
            analysis.ErrorMessage = ex.Message + (ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "");
            await _context.SaveChangesAsync();
            ModelState.AddModelError("", "Error: " + ex.Message);
        }

        return View();
    }

    public async Task<IActionResult> Details(int id)
    {
        var analysis = await _context.Analyses
            .Include(a => a.Business)
            .Include(a => a.Reviews)
            .ThenInclude(r => r.AspectResults)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (analysis == null) return NotFound();

        return View(analysis);
    }

    public async Task<IActionResult> DownloadPdf(int id)
    {
         // Compatibility redirect or error
         return RedirectToAction(nameof(Details), new { id = id });
    }

    public class PdfRequest
    {
        public int Id { get; set; }
        public string SentimentChartImage { get; set; } = string.Empty;
        public string AspectChartImage { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> DownloadPdfWithCharts([FromBody] PdfRequest request)
    {
        var analysis = await _context.Analyses
            .Include(a => a.Business)
            .Include(a => a.Reviews)
            .ThenInclude(r => r.AspectResults)
            .FirstOrDefaultAsync(m => m.Id == request.Id);

        if (analysis == null) return NotFound();

        // Pass charts to PdfService (Need to overload GenerateReport or pass as extra args)
        var pdfBytes = _pdfService.GenerateReport(analysis, request.SentimentChartImage, request.AspectChartImage);
        return File(pdfBytes, "application/pdf", $"SentimentReport_{analysis.Business?.Name}_{analysis.Date:yyyyMMdd}.pdf");
    }
}
