using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class ModerationController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Moderation Dashboard";
            return View();
        }
    }
}
