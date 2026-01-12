using Microsoft.AspNetCore.Mvc;

namespace SentimentHub.Web.Controllers;

public class TestController : Controller
{
    [HttpGet]
    public string Index()
    {
        return "MVC ROUTING IS WORKING - TEST CONTROLLER";
    }
}
