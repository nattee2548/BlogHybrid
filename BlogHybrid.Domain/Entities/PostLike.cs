using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Domain.Entities
{
    public class PostLike
    {
        public int PostId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Post Post { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
