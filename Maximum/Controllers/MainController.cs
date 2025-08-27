using Microsoft.AspNetCore.Mvc;

namespace Maximum.Controllers;

public class MainController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}