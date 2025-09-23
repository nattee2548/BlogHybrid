// BlogHybrid.Web/Areas/Admin/Controllers/ImageController.cs
using BlogHybrid.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHybrid.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class ImageController : Controller
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(
            IImageService imageService,
            ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        // POST: /Admin/Image/Upload
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, string folder = "categories")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "กรุณาเลือกไฟล์รูปภาพ" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return Json(new
                    {
                        success = false,
                        message = $"ไฟล์ประเภท {extension} ไม่ได้รับอนุญาต กรุณาใช้ .jpg, .png, .gif หรือ .webp"
                    });
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return Json(new
                    {
                        success = false,
                        message = "ขนาดไฟล์ไม่ควรเกิน 5MB"
                    });
                }

                // Upload to R2
                var imagePath = await _imageService.UploadAsync(file, folder);
                var imageUrl = _imageService.GetImageUrl(imagePath);

                _logger.LogInformation("Image uploaded successfully: {ImagePath}", imagePath);

                return Json(new
                {
                    success = true,
                    message = "อัพโหลดรูปภาพสำเร็จ",
                    imagePath = imagePath,
                    imageUrl = imageUrl
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid file upload attempt: {Message}", ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image");
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการอัพโหลดรูปภาพ" });
            }
        }

        // DELETE: /Admin/Image/Delete
        [HttpDelete]
        public async Task<IActionResult> Delete(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    return Json(new { success = false, message = "ไม่พบเส้นทางรูปภาพ" });
                }

                var result = await _imageService.DeleteAsync(imagePath);

                if (result)
                {
                    _logger.LogInformation("Image deleted successfully: {ImagePath}", imagePath);
                    return Json(new { success = true, message = "ลบรูปภาพสำเร็จ" });
                }
                else
                {
                    return Json(new { success = false, message = "ไม่สามารถลบรูปภาพได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image: {ImagePath}", imagePath);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการลบรูปภาพ" });
            }
        }

        // POST: /Admin/Image/UploadCategory
        [HttpPost]
        public async Task<IActionResult> UploadCategory(IFormFile file)
        {
            return await Upload(file, "categories");
        }

        // POST: /Admin/Image/UploadPost  
        [HttpPost]
        public async Task<IActionResult> UploadPost(IFormFile file)
        {
            return await Upload(file, "posts");
        }

        // POST: /Admin/Image/UploadAvatar
        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            return await Upload(file, "avatars");
        }

        // GET: /Admin/Image/Test (สำหรับทดสอบ)
        [HttpGet]
        public IActionResult Test()
        {
            return View();
        }
    }
}