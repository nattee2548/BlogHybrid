using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.User.Controllers
{
    public class PostsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
