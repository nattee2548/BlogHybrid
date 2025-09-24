// BlogHybrid.Web/Areas/Admin/CategoryController.cs
using BlogHybrid.Application.Commands.Category;
using BlogHybrid.Application.DTOs.Category;
using BlogHybrid.Application.Queries.Category;
using BlogHybrid.Web.Models.ViewModels.Admin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(
            IMediator mediator,
            ILogger<CategoryController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        #region Index & List

        // GET: /Admin/Category
        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1,
            int pageSize = 10,
            string? search = null,
            bool? isActive = null,
            string sortBy = "SortOrder",
            string sortDirection = "asc")
        {
            try
            {
                var query = new GetCategoriesQuery
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    SearchTerm = search,
                    IsActive = isActive,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                };

                var result = await _mediator.Send(query);

                var viewModel = new CategoryIndexViewModel
                {
                    Categories = result,
                    CurrentPage = page,
                    PageSize = pageSize,
                    SearchTerm = search,
                    IsActiveFilter = isActive,
                    SortBy = sortBy,
                    SortDirection = sortDirection
                };

                // For HTMX requests, return partial view
                if (Request.Headers.ContainsKey("HX-Request"))
                {
                    return PartialView("_CategoryTable", viewModel);
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูลหมวดหมู่";
                return View(new CategoryIndexViewModel());
            }
        }

        #endregion

        #region Create

        // GET: /Admin/Category/Create
        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new CreateCategoryViewModel
            {
                Color = "#0066cc",
                IsActive = true,
                SortOrder = 0 // Will auto-assign in handler
            };

            return View(viewModel);
        }

        // POST: /Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (!string.IsNullOrEmpty(model.Slug))
                {
                    var slugExistsQuery = new CheckCategorySlugExistsQuery
                    {
                        Slug = model.Slug,
                        ExcludeId = null
                    };

                    var slugExists = await _mediator.Send(slugExistsQuery);
                    if (slugExists)
                    {
                        ModelState.AddModelError(nameof(model.Slug), "URL Slug นี้ถูกใช้แล้ว");
                        return View(model);
                    }
                }

                var command = new CreateCategoryCommand
                {
                    Name = model.Name,
                    Slug = model.Slug,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl,
                    Color = model.Color,
                    IsActive = model.IsActive,
                    SortOrder = model.SortOrder
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "สร้างหมวดหมู่เรียบร้อยแล้ว";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                ModelState.AddModelError(string.Empty, "เกิดข้อผิดพลาดในการสร้างหมวดหมู่");
            }

            return View(model);
        }

        #endregion

        #region Edit

        // GET: /Admin/Category/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var query = new GetCategoryByIdQuery { Id = id };
                var category = await _mediator.Send(query);

                if (category == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบหมวดหมู่ที่ต้องการแก้ไข";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new EditCategoryViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ImageUrl = category.ImageUrl,
                    Color = category.Color,
                    IsActive = category.IsActive,
                    SortOrder = category.SortOrder,
                    CurrentSlug = category.Slug,
                    PostCount = category.PostCount
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category for edit: {CategoryId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูลหมวดหมู่";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCategoryViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var command = new UpdateCategoryCommand
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl,
                    Color = model.Color,
                    IsActive = model.IsActive,
                    SortOrder = model.SortOrder
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "แก้ไขหมวดหมู่เรียบร้อยแล้ว";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", id);
                ModelState.AddModelError(string.Empty, "เกิดข้อผิดพลาดในการแก้ไขหมวดหมู่");
            }

            return View(model);
        }

        #endregion

        #region Delete

        // POST: /Admin/Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, bool forceDelete = false)
        {
            try
            {
                var command = new DeleteCategoryCommand
                {
                    Id = id,
                    ForceDelete = forceDelete
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "ลบหมวดหมู่เรียบร้อยแล้ว";
                }
                else
                {
                    if (result.HasPosts)
                    {
                        TempData["ErrorMessage"] = $"ไม่สามารถลบได้ เนื่องจากมีบทความ {result.PostCount} รายการ";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = string.Join(", ", result.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการลบหมวดหมู่";
            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region AJAX Actions

        // POST: /Admin/Category/ToggleStatus
        [HttpPost]
        public async Task<IActionResult> ToggleStatus([FromBody] ToggleStatusRequest request)
        {
            try
            {
                var command = new ToggleCategoryStatusCommand
                {
                    Id = request.Id,
                    IsActive = request.IsActive
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Json(new { success = true, message = "เปลี่ยนสถานะเรียบร้อยแล้ว", newStatus = result.NewStatus });
                }

                return Json(new { success = false, message = string.Join(", ", result.Errors) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", request.Id);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการเปลี่ยนสถานะ" });
            }
        }

        // POST: /Admin/Category/Reorder
        [HttpPost]
        public async Task<IActionResult> Reorder([FromBody] List<ReorderItem> items)
        {
            try
            {
                var command = new ReorderCategoriesCommand
                {
                    Categories = items.Select(i => new CategoryOrderItem
                    {
                        Id = i.Id,
                        SortOrder = i.Order
                    }).ToList()
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    return Json(new { success = true, message = "เรียงลำดับเรียบร้อยแล้ว" });
                }

                return Json(new { success = false, message = string.Join(", ", result.Errors) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering categories");
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการเรียงลำดับ" });
            }
        }

        // GET: /Admin/Category/CheckSlug
        [HttpGet]
        public async Task<IActionResult> CheckSlug(string slug, int? excludeId = null)
        {
            try
            {
                var query = new CheckCategorySlugExistsQuery
                {
                    Slug = slug,
                    ExcludeId = excludeId
                };

                var exists = await _mediator.Send(query);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slug: {Slug}", slug);
                return Json(new { exists = true }); // Safe default
            }
        }

        #endregion

        #region Helper Classes

        public class ToggleStatusRequest
        {
            public int Id { get; set; }
            public bool IsActive { get; set; }
        }

        public class ReorderItem
        {
            public int Id { get; set; }
            public int Order { get; set; }
        }

        #endregion
    }
}