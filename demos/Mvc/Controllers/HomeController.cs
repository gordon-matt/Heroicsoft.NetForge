using Microsoft.AspNetCore.Mvc;

namespace NetForge.Demo.Mvc.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
