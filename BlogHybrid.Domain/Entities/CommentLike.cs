using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Domain.Entities
{
    public class CommentLike
    {
        public int CommentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Comment Comment { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
