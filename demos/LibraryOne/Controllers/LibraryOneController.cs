using Microsoft.AspNetCore.Mvc;

namespace NetForge.Demo.LibraryOne.Controllers;

public sealed class LibraryOneController : Controller
{
    [HttpGet("/admin/library-one")]
    public IActionResult Index() => View();
}
