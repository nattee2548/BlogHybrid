// BlogHybrid.Application/Handlers/Tag/UpdateTagHandler.cs
using BlogHybrid.Application.Commands.Tag;
using BlogHybrid.Application.Common;
using BlogHybrid.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BlogHybrid.Application.Handlers.Tag
{
    public class UpdateTagHandler : IRequestHandler<UpdateTagCommand, UpdateTagResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateTagHandler> _logger;

        public UpdateTagHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateTagHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UpdateTagResult> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var tag = await _unitOfWork.Tags.GetByIdAsync(request.Id, cancellationToken);

                if (tag == null)
                {
                    return new UpdateTagResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่พบแท็กที่ต้องการแก้ไข" }
                    };
                }

                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return new UpdateTagResult
                    {
                        Success = false,
                        Errors = new List<string> { "ชื่อแท็กเป็นข้อมูลที่จำเป็น" }
                    };
                }

                var slug = tag.Slug;
                if (tag.Name != request.Name.Trim())
                {
                    slug = await SlugGenerator.GenerateUniqueSlug(
                        request.Name,
                        async (slugToCheck, excludeId) =>
                            await _unitOfWork.Tags.SlugExistsAsync(slugToCheck, excludeId, cancellationToken),
                        tag.Id
                    );
                }

                tag.Name = request.Name.Trim();
                tag.Slug = slug;

                await _unitOfWork.Tags.UpdateAsync(tag, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Tag updated: {TagId} - {TagName}", tag.Id, tag.Name);

                return new UpdateTagResult
                {
                    Success = true,
                    Slug = tag.Slug,
                    Message = "แก้ไขแท็กเรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag: {TagId}", request.Id);
                return new UpdateTagResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการแก้ไขแท็ก" }
                };
            }
        }
    }
}