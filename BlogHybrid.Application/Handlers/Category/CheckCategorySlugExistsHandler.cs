using BlogHybrid.Application.Interfaces.Repositories;
using BlogHybrid.Application.Queries.Category;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Handlers.Category
{
    public class CheckCategorySlugExistsHandler : IRequestHandler<CheckCategorySlugExistsQuery, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CheckCategorySlugExistsHandler> _logger;

        public CheckCategorySlugExistsHandler(
            IUnitOfWork unitOfWork,
            ILogger<CheckCategorySlugExistsHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(CheckCategorySlugExistsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _unitOfWork.Categories.SlugExistsAsync(request.Slug, request.ExcludeId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slug exists: {Slug}", request.Slug);
                return true; // Safe default - assume exists
            }
        }
    }
}
