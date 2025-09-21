using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Admin Dashboard";
            return View();
        }
    }
}
