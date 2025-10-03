// BlogHybrid.API/Controllers/Admin/CategoriesController.cs
using BlogHybrid.Application.Commands.Category;
using BlogHybrid.Application.DTOs.Category;
using BlogHybrid.Application.Interfaces.Services;
using BlogHybrid.Application.Queries.Category;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")] // ✅ เฉพาะ Admin เท่านั้น
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IImageService _imageService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            IMediator mediator,
            IImageService imageService,
            ILogger<CategoriesController> logger)
        {
            _mediator = mediator;
            _imageService = imageService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of categories with filters
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/admin/categories?pageNumber=1&amp;pageSize=10&amp;searchTerm=tech
        /// 
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(CategoryListDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories([FromQuery] GetCategoriesQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูลหมวดหมู่"
                });
            }
        }

        /// <summary>
        /// Get active categories only (for dropdown/select)
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveCategories([FromQuery] bool orderByName = false)
        {
            try
            {
                var query = new GetActiveCategoriesQuery { OrderByName = orderByName };
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active categories");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูลหมวดหมู่ที่เปิดใช้งาน"
                });
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var query = new GetCategoryByIdQuery { Id = id };
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ไม่พบหมวดหมู่ที่ต้องการ"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category {CategoryId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูลหมวดหมู่"
                });
            }
        }

        /// <summary>
        /// Create new category
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/admin/categories
        ///     {
        ///        "name": "Technology",
        ///        "description": "Tech news and articles",
        ///        "color": "#0066cc",
        ///        "isActive": true,
        ///        "sortOrder": 1
        ///     }
        /// 
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(CreateCategoryResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ข้อมูลไม่ถูกต้อง",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "สร้างหมวดหมู่ไม่สำเร็จ",
                        errors = result.Errors
                    });
                }

                _logger.LogInformation("Category created: {CategoryId} - {CategoryName}",
                    result.CategoryId, command.Name);

                return CreatedAtAction(
                    nameof(GetCategory),
                    new { id = result.CategoryId },
                    new
                    {
                        success = true,
                        message = "สร้างหมวดหมู่สำเร็จ",
                        data = result
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการสร้างหมวดหมู่"
                });
            }
        }

        /// <summary>
        /// Update existing category
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryCommand command)
        {
            try
            {
                if (id != command.Id)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID ไม่ตรงกัน"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ข้อมูลไม่ถูกต้อง",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "แก้ไขหมวดหมู่ไม่สำเร็จ",
                        errors = result.Errors
                    });
                }

                _logger.LogInformation("Category updated: {CategoryId}", id);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการแก้ไขหมวดหมู่"
                });
            }
        }

        /// <summary>
        /// Delete category
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id, [FromQuery] bool forceDelete = false)
        {
            try
            {
                var command = new DeleteCategoryCommand
                {
                    Id = id,
                    ForceDelete = forceDelete
                };

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    // ถ้ามี posts อยู่ ให้ส่ง status 409 Conflict
                    if (result.HasPosts)
                    {
                        return Conflict(new
                        {
                            success = false,
                            message = result.Errors.FirstOrDefault(),
                            hasPosts = true,
                            postCount = result.PostCount
                        });
                    }

                    return BadRequest(new
                    {
                        success = false,
                        message = "ลบหมวดหมู่ไม่สำเร็จ",
                        errors = result.Errors
                    });
                }

                _logger.LogInformation("Category deleted: {CategoryId}", id);

                return Ok(new
                {
                    success = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการลบหมวดหมู่"
                });
            }
        }

        /// <summary>
        /// Toggle category active status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] ToggleCategoryStatusCommand command)
        {
            try
            {
                if (id != command.Id)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID ไม่ตรงกัน"
                    });
                }

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "เปลี่ยนสถานะไม่สำเร็จ",
                        errors = result.Errors
                    });
                }

                _logger.LogInformation("Category status toggled: {CategoryId} -> {Status}",
                    id, result.NewStatus);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = new { isActive = result.NewStatus }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status {CategoryId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการเปลี่ยนสถานะ"
                });
            }
        }

        /// <summary>
        /// Reorder categories
        /// </summary>
        [HttpPut("reorder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReorderCategories([FromBody] ReorderCategoriesCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "เรียงลำดับไม่สำเร็จ",
                        errors = result.Errors
                    });
                }

                _logger.LogInformation("Categories reordered");

                return Ok(new
                {
                    success = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering categories");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการเรียงลำดับ"
                });
            }
        }

        /// <summary>
        /// Check if slug exists
        /// </summary>
        [HttpGet("check-slug/{slug}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckSlugExists(string slug, [FromQuery] int? excludeId = null)
        {
            try
            {
                var query = new CheckCategorySlugExistsQuery
                {
                    Slug = slug,
                    ExcludeId = excludeId
                };

                var exists = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    exists = exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slug {Slug}", slug);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการตรวจสอบ slug"
                });
            }
        }

        /// <summary>
        /// Upload category image
        /// </summary>
        [HttpPost("upload-image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ไม่พบไฟล์ที่อัปโหลด"
                    });
                }

                // ตรวจสอบประเภทไฟล์
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "รองรับเฉพาะไฟล์รูปภาพ (JPG, PNG, WebP, GIF)"
                    });
                }

                // ตรวจสอบขนาดไฟล์ (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ขนาดไฟล์ต้องไม่เกิน 5MB"
                    });
                }

                var imagePath = await _imageService.UploadAsync(file, "categories");
                var imageUrl = _imageService.GetImageUrl(imagePath);

                _logger.LogInformation("Category image uploaded: {ImagePath}", imagePath);

                return Ok(new
                {
                    success = true,
                    message = "อัปโหลดรูปภาพสำเร็จ",
                    data = new
                    {
                        path = imagePath,
                        url = imageUrl
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading category image");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการอัปโหลดรูปภาพ"
                });
            }
        }

        /// <summary>
        /// Delete category image
        /// </summary>
        [HttpDelete("delete-image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteImage([FromQuery] string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ไม่พบ path ของรูปภาพ"
                    });
                }

                var deleted = await _imageService.DeleteAsync(imagePath);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ไม่พบรูปภาพที่ต้องการลบ"
                    });
                }

                _logger.LogInformation("Category image deleted: {ImagePath}", imagePath);

                return Ok(new
                {
                    success = true,
                    message = "ลบรูปภาพสำเร็จ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category image: {ImagePath}", imagePath);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการลบรูปภาพ"
                });
            }
        }
    }
}