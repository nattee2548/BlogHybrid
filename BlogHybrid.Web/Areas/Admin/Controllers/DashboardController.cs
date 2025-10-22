using BlogHybrid.Application.Queries.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IMediator mediator,
            ILogger<DashboardController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            try
            {                
                var query = new GetUserStatisticsQuery();
                var stats = await _mediator.Send(query);

                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";

                // ส่ง empty stats ถ้าเกิด error
                return View(new UserStatisticsResult());
            }
        }
    }
}