using BlogHybrid.Application.DTOs.Category;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Queries.Category
{
    public class GetActiveCategoriesQuery : IRequest<List<CategoryDto>>
    {
        public bool OrderByName { get; set; } = false; // false = order by SortOrder, true = order by Name
    }
}
