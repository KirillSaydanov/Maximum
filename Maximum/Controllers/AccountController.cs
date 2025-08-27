using Microsoft.AspNetCore.Mvc;

namespace Maximum.Controllers;

public class AccountController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}