// BlogHybrid.Web/Controllers/PostController.cs
using BlogHybrid.Application.Commands.Comment;
using BlogHybrid.Application.Queries.Post;
using BlogHybrid.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Controllers
{
    /// <summary>
    /// Controller สำหรับหน้าสาธารณะของโพสต์ (คนทั่วไปดูได้)
    /// </summary>
    public class PostController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PostController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(
            IMediator mediator,
            ILogger<PostController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _mediator = mediator;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: /Post/Details/{slug}
        [HttpGet("post/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(string slug)
        {
            try
            {
                // ดึง current user id (ถ้ามี)
                var currentUserId = User.Identity?.IsAuthenticated == true
                    ? _userManager.GetUserId(User)
                    : null;

                var query = new GetPostDetailBySlugQuery
                {
                    Slug = slug,
                    CurrentUserId = currentUserId
                };

                var post = await _mediator.Send(query);

                if (post == null)
                {
                    _logger.LogWarning("Post not found: {Slug}", slug);
                    TempData["ErrorMessage"] = "ไม่พบโพสต์ที่ค้นหา";
                    return RedirectToAction("Index", "Home");
                }

                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading post details: {Slug}", slug);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดโพสต์";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Post/AddComment
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(
            int postId,
            string content,
            int? parentCommentId = null,
            string? returnSlug = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนแสดงความคิดเห็น";
                    return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    TempData["ErrorMessage"] = "กรุณาใส่ความคิดเห็น";
                    return RedirectToAction(nameof(Details), new { slug = returnSlug });
                }

                var command = new AddCommentCommand
                {
                    PostId = postId,
                    Content = content,
                    AuthorId = userId,
                    ParentCommentId = parentCommentId
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "เพิ่มความคิดเห็นสำเร็จ";
                    _logger.LogInformation("Comment added: CommentId={CommentId}", result.CommentId);
                }
                else
                {
                    TempData["ErrorMessage"] = string.Join(", ", result.Errors);
                }

                // กลับไปที่โพสต์ พร้อม scroll ไปที่ comment
                TempData["ScrollToCommentId"] = result.CommentId;
                return RedirectToAction(nameof(Details), new { slug = returnSlug });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment to post: {PostId}", postId);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการเพิ่มความคิดเห็น";
                return RedirectToAction(nameof(Details), new { slug = returnSlug });
            }
        }
    }
}