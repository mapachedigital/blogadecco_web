using BlogAdecco.Data;
using BlogAdecco.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogAdecco.Areas.Admin;

[Area("Admin")]
[Authorize]
public class HomeController : Controller
{
    public async Task<IActionResult> Index()
    {       
        return View();
    }
}
