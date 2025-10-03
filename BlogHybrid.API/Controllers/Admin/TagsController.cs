// BlogHybrid.API/Controllers/Admin/TagsController.cs (Part 1 - ครึ่งแรก)
using BlogHybrid.Application.Commands.Tag;
using BlogHybrid.Application.DTOs.Tag;
using BlogHybrid.Application.Queries.Tag;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogHybrid.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class TagsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TagsController> _logger;

        public TagsController(
            IMediator mediator,
            ILogger<TagsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of tags with filters
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(TagListDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTags([FromQuery] GetTagsQuery query)
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
                _logger.LogError(ex, "Error getting tags");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูลแท็ก"
                });
            }
        }

        /// <summary>
        /// Get tag by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTag(int id)
        {
            try
            {
                var query = new GetTagByIdQuery { Id = id };
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "ไม่พบแท็กที่ต้องการ"
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
                _logger.LogError(ex, "Error getting tag {TagId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการดึงข้อมูลแท็ก"
                });
            }
        }

        /// <summary>
        /// Search tags (for autocomplete)
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchTags([FromQuery] string q, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return Ok(new
                    {
                        success = true,
                        data = new List<TagDto>()
                    });
                }

                var query = new SearchTagsQuery
                {
                    SearchTerm = q,
                    Limit = limit
                };
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tags: {SearchTerm}", q);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการค้นหาแท็ก"
                });
            }
        }

        /// <summary>
        /// Find similar tags (AI detection)
        /// </summary>
        [HttpGet("similar")]
        public async Task<IActionResult> FindSimilarTags([FromQuery] string name, [FromQuery] int limit = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "กรุณาระบุชื่อแท็ก"
                    });
                }

                var query = new FindSimilarTagsQuery
                {
                    TagName = name,
                    Limit = limit
                };
                var result = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar tags: {TagName}", name);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการหาแท็กที่คล้ายกัน"
                });
            }
        }


        // BlogHybrid.API/Controllers/Admin/TagsController.cs (Part 2 - เพิ่มต่อในไฟล์เดิม)
        // ⚠️ เพิ่ม methods เหล่านี้ต่อจาก FindSimilarTags ในไฟล์เดิม (ก่อน closing brace สุดท้าย)

        /// <summary>
        /// Create new tag
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CreateTagResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagCommand command)
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

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                command.CreatedBy = userId;

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "สร้างแท็กไม่สำเร็จ",
                        errors = result.Errors,
                        suggestedTags = result.SimilarTags
                    });
                }

                _logger.LogInformation("Tag created: {TagId} - {TagName} by {UserId}",
                    result.TagId, command.Name, userId);

                return CreatedAtAction(
                    nameof(GetTag),
                    new { id = result.TagId },
                    new
                    {
                        success = true,
                        message = result.Message,
                        data = result,
                        warnings = result.SimilarTags
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการสร้างแท็ก"
                });
            }
        }

        /// <summary>
        /// Bulk create tags
        /// </summary>
        [HttpPost("bulk")]
        [ProducesResponseType(typeof(BulkCreateTagsResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> BulkCreateTags([FromBody] BulkCreateTagsCommand command)
        {
            try
            {
                if (command.TagNames == null || !command.TagNames.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "กรุณาระบุชื่อแท็กอย่างน้อย 1 ชื่อ"
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                command.CreatedBy = userId;

                var result = await _mediator.Send(command);

                return Ok(new
                {
                    success = result.Success,
                    message = $"สร้างแท็กใหม่ {result.CreatedTags.Count} ชื่อ, พบแท็กที่มีอยู่แล้ว {result.ExistingTags.Count} ชื่อ",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk create tags");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการสร้างแท็กแบบ bulk"
                });
            }
        }

        /// <summary>
        /// Update existing tag
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] UpdateTagCommand command)
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
                        message = "แก้ไขแท็กไม่สำเร็จ",
                        errors = result.Errors
                    });
                }

                _logger.LogInformation("Tag updated: {TagId}", id);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag {TagId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการแก้ไขแท็ก"
                });
            }
        }

        /// <summary>
        /// Delete tag
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteTag(int id, [FromQuery] bool forceDelete = false)
        {
            try
            {
                var command = new DeleteTagCommand
                {
                    Id = id,
                    ForceDelete = forceDelete
                };

                var result = await _mediator.Send(command);

                if (!result.Success)
                {
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
                        message = "ลบแท็กไม่สำเร็จ",
                        errors = result.Errors
                    });
                }

                _logger.LogInformation("Tag deleted: {TagId}", id);

                return Ok(new
                {
                    success = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag {TagId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการลบแท็ก"
                });
            }
        }

        /// <summary>
        /// Merge two tags (Admin only)
        /// </summary>
        [HttpPost("merge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MergeTags([FromBody] MergeTagsCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "รวมแท็กไม่สำเร็จ",
                        errors = result.Errors
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    postsMerged = result.PostsMerged
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging tags");
                return StatusCode(500, new
                {
                    success = false,
                    message = "เกิดข้อผิดพลาดในการรวมแท็ก"
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
                var query = new CheckTagSlugExistsQuery
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
    }
}
    
