// BlogHybrid.Web/Areas/Admin/Controllers/CategoriesController.cs
using BlogHybrid.Application.Commands.Category;
using BlogHybrid.Application.Interfaces.Services; // ✅ เพิ่ม
using BlogHybrid.Application.Queries.Category;
using BlogHybrid.Web.Areas.Admin.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CategoriesController> _logger;
        private readonly IImageService _imageService; 

        public CategoriesController(
            IMediator mediator,
            ILogger<CategoriesController> logger,
            IImageService imageService) 
        {
            _mediator = mediator;
            _logger = logger;
            _imageService = imageService; // ✅ เพิ่ม
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Index(string? search, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var query = new GetCategoriesQuery
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = search,
                    IsActive = null // แสดงทั้งหมด
                };

                var result = await _mediator.Send(query);

                var viewModel = new CategoriesListViewModel
                {
                    Categories = result.Categories.Select(c => new CategoryItemViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Slug = c.Slug,
                        Description = c.Description,
                        Color = c.Color,
                        ImageUrl = c.ImageUrl,
                        IsActive = c.IsActive,
                        SortOrder = c.SortOrder,
                        PostCount = c.PostCount,
                        CommunityCount = c.CommunityCount,
                        CreatedAt = c.CreatedAt,
                        ParentCategoryId = c.ParentCategoryId,           // เพิ่ม
                        ParentCategoryName = c.ParentCategoryName        // เพิ่ม
                    }).ToList(),
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    SearchTerm = search
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories list");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดรายการหมวดหมู่";
                return View(new CategoriesListViewModel());
            }
        }

        // GET: /Admin/Categories/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var query = new GetCategoryByIdQuery { Id = id };
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบหมวดหมู่ที่ต้องการ";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new CategoryDetailsViewModel
                {
                    Id = result.Id,
                    Name = result.Name,
                    Slug = result.Slug,
                    Description = result.Description,
                    Color = result.Color,
                    ImageUrl = result.ImageUrl,
                    IsActive = result.IsActive,
                    SortOrder = result.SortOrder,
                    PostCount = result.PostCount,
                    CommunityCount = result.CommunityCount,
                    CreatedAt = result.CreatedAt,
                    ParentCategoryId = result.ParentCategoryId,          // เพิ่ม
                    ParentCategoryName = result.ParentCategoryName       // เพิ่ม
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading category details for ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Admin/Categories/Create
        public async Task<IActionResult> Create()
        {
            var parentCategoriesQuery = new GetParentCategoriesQuery { ActiveOnly = false };
            var parentCategories = await _mediator.Send(parentCategoriesQuery);

            ViewBag.ParentCategories = parentCategories;

            return View(new CreateCategoryViewModel());
        }

        // POST: /Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // โหลด parent categories กลับมาถ้า validation ผิด
                var parentCategoriesQuery = new GetParentCategoriesQuery { ActiveOnly = false };
                var parentCategories = await _mediator.Send(parentCategoriesQuery);
                ViewBag.ParentCategories = parentCategories;

                return View(model);
            }

            try
            {
                string? imageKey = null;

                // Upload image ถ้ามี
                if (model.ImageFile != null)
                {
                    imageKey = await _imageService.UploadAsync(model.ImageFile, "categories");
                }

                var command = new CreateCategoryCommand
                {
                    Name = model.Name,
                    Description = model.Description,
                    Color = model.Color,
                    ImageUrl = !string.IsNullOrEmpty(imageKey)
                        ? imageKey
                        : model.ImageUrl,
                    IsActive = model.IsActive,
                    SortOrder = model.SortOrder,
                    ParentCategoryId = model.ParentCategoryId  // ← เพิ่มบรรทัดนี้
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }

                    TempData["ErrorMessage"] = result.Message;

                    // โหลด parent categories กลับมา
                    var parentCategoriesQuery = new GetParentCategoriesQuery { ActiveOnly = false };
                    var parentCategories = await _mediator.Send(parentCategoriesQuery);
                    ViewBag.ParentCategories = parentCategories;

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการสร้างหมวดหมู่";

                var parentCategoriesQuery = new GetParentCategoriesQuery { ActiveOnly = false };
                var parentCategories = await _mediator.Send(parentCategoriesQuery);
                ViewBag.ParentCategories = parentCategories;

                return View(model);
            }
        }

        // GET: /Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var query = new GetCategoryByIdQuery { Id = id };
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบหมวดหมู่ที่ต้องการ";
                    return RedirectToAction(nameof(Index));
                }

                // โหลดหมวดหมู่หลัก (ยกเว้นตัวมันเอง)
                var parentCategoriesQuery = new GetParentCategoriesQuery { ActiveOnly = false };
                var parentCategories = await _mediator.Send(parentCategoriesQuery);

                // กรองออกตัวมันเอง (ป้องกัน circular reference)
                ViewBag.ParentCategories = parentCategories.Where(c => c.Id != id).ToList();

                var viewModel = new EditCategoryViewModel
                {
                    Id = result.Id,
                    ParentCategoryId = result.ParentCategoryId,  // ← เพิ่มบรรทัดนี้
                    Name = result.Name,
                    Description = result.Description,
                    Color = result.Color,
                    ImageUrl = result.ImageUrl,
                    CurrentImageUrl = result.ImageUrl,
                    IsActive = result.IsActive,
                    SortOrder = result.SortOrder
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for category ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCategoryViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // โหลด parent categories กลับมา
                var parentCategoriesQuery = new GetParentCategoriesQuery { ActiveOnly = false };
                var parentCategories = await _mediator.Send(parentCategoriesQuery);
                ViewBag.ParentCategories = parentCategories.Where(c => c.Id != id).ToList();

                return View(model);
            }

            try
            {
                string? imageKey = model.ImageUrl;

                // Upload image ใหม่ถ้ามี
                if (model.ImageFile != null)
                {
                    imageKey = await _imageService.UploadAsync(model.ImageFile, "categories");
                }

                var command = new UpdateCategoryCommand
                {
                    Id = model.Id,
                    ParentCategoryId = model.ParentCategoryId,  // ← เพิ่มบรรทัดนี้
                    Name = model.Name,
                    Description = model.Description,
                    Color = model.Color,
                    ImageUrl = imageKey,
                    IsActive = model.IsActive,
                    SortOrder = model.SortOrder
                };

                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }

                    TempData["ErrorMessage"] = result.Message;

                    var parentCategoriesQuery = new GetParentCategoriesQuery { ActiveOnly = false };
                    var parentCategories = await _mediator.Send(parentCategoriesQuery);
                    ViewBag.ParentCategories = parentCategories.Where(c => c.Id != id).ToList();

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการอัปเดตหมวดหมู่";

                var parentCategoriesQuery = new GetParentCategoriesQuery { ActiveOnly = false };
                var parentCategories = await _mediator.Send(parentCategoriesQuery);
                ViewBag.ParentCategories = parentCategories.Where(c => c.Id != id).ToList();

                return View(model);
            }
        }

        // POST: /Admin/Categories/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var command = new ToggleCategoryStatusCommand { CategoryId = id };
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "ไม่สามารถเปลี่ยนสถานะได้";
                }

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling active status for category ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการเปลี่ยนสถานะ";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var command = new DeleteCategoryCommand { Id = id };
                var result = await _mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "ลบหมวดหมู่สำเร็จ";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = result.Errors?.FirstOrDefault() ?? "ไม่สามารถลบหมวดหมู่ได้";
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category ID: {id}");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการลบหมวดหมู่";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}