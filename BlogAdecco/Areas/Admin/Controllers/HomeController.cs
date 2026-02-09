using BlogAdecco.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogAdecco.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class HomeController(IUserUtils _userUtils) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await _userUtils.GetUserAsync();
        var isAdmin = await _userUtils.IsAdminAsync(false);

        ViewData["IsAdmin"] = isAdmin;

        return View();
    }
}
