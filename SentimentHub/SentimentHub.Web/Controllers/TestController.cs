using Microsoft.AspNetCore.Mvc;

namespace SentimentHub.Web.Controllers;

public class TestController : Controller
{
    private readonly Microsoft.AspNetCore.Identity.UserManager<SentimentHub.Web.Models.ApplicationUser> _userManager;

    public TestController(Microsoft.AspNetCore.Identity.UserManager<SentimentHub.Web.Models.ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public string Index()
    {
        return "MVC ROUTING IS WORKING - TEST CONTROLLER";
    }

    [HttpGet]
    public async Task<string> VerifyAll()
    {
        var users = _userManager.Users.ToList();
        int count = 0;
        foreach (var user in users)
        {
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                count++;
            }
        }
        return $"{count} existing users have been verified.";
    }
}
