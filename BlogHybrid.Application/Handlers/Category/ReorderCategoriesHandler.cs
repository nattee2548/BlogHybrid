using BlogHybrid.Application.Commands.Category;
using BlogHybrid.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Handlers.Category
{
    public class ReorderCategoriesHandler : IRequestHandler<ReorderCategoriesCommand, ReorderCategoriesResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReorderCategoriesHandler> _logger;

        public ReorderCategoriesHandler(
            IUnitOfWork unitOfWork,
            ILogger<ReorderCategoriesHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ReorderCategoriesResult> Handle(ReorderCategoriesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Categories == null || !request.Categories.Any())
                {
                    return new ReorderCategoriesResult
                    {
                        Success = false,
                        Errors = new List<string> { "ไม่มีข้อมูลการเรียงลำดับ" }
                    };
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                foreach (var item in request.Categories)
                {
                    var category = await _unitOfWork.Categories.GetByIdAsync(item.Id, cancellationToken);
                    if (category != null)
                    {
                        category.SortOrder = item.SortOrder;
                        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation("Categories reordered successfully");

                return new ReorderCategoriesResult
                {
                    Success = true,
                    Message = "เรียงลำดับหมวดหมู่เรียบร้อยแล้ว"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, "Error reordering categories");
                return new ReorderCategoriesResult
                {
                    Success = false,
                    Errors = new List<string> { "เกิดข้อผิดพลาดในการเรียงลำดับ" }
                };
            }
        }
    }
}
