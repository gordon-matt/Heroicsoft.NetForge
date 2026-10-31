using Microsoft.AspNetCore.Mvc;

namespace NetForge.Demo.LibraryTwo.Controllers;

public sealed class LibraryTwoController : Controller
{
    [HttpGet("/admin/library-two")]
    public IActionResult Index() => View();
}
