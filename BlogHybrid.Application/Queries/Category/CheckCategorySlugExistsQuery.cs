using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Application.Queries.Category
{
    public class CheckCategorySlugExistsQuery : IRequest<bool>
    {
        public string Slug { get; set; } = string.Empty;
        public int? ExcludeId { get; set; } // สำหรับการ update ไม่ให้เช็คตัวเอง
    }
}
