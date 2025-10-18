using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.User.Controllers
{
    public class CommunitiesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
