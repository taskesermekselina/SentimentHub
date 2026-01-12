using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SentimentHub.Web.Models;
using SentimentHub.Web.Data;
using Microsoft.AspNetCore.Identity;

namespace SentimentHub.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SentimentHub.Web.Data.ApplicationDbContext _context;
    private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, SentimentHub.Web.Data.ApplicationDbContext context, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            // If not logged in, return empty list or specific view model indicating no data
            return View(new List<Analysis>());
        }

        var userId = _userManager.GetUserId(User);

        var recentAnalyses = _context.Analyses
            .Include(a => a.Business)
            .Where(a => a.Business.OwnerId == userId)
            .OrderByDescending(a => a.Date)
            .Take(5)
            .ToList();
            
        return View(recentAnalyses);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
