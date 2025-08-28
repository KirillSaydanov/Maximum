using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Maximum.Controllers;

[Authorize]
public class MainController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}