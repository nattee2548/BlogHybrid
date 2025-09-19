using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Domain.Entities
{
    public class PostTag
    {
        public int PostId { get; set; }
        public int TagId { get; set; }

        // Navigation properties
        public virtual Post Post { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}
