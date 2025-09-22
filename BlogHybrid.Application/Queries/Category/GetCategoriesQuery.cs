using BlogHybrid.Application.DTOs.Category;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Queries.Category
{
    public class GetCategoriesQuery : IRequest<CategoryListDto>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "SortOrder";
        public string SortDirection { get; set; } = "asc";
    }
   

}
