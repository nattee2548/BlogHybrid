using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.User.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
