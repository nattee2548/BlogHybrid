using MediatR;

namespace BlogHybrid.Application.Queries.Post
{
    /// <summary>
    /// Query สำหรับดึง Public Posts (Posts ที่เผยแพร่แล้ว สำหรับหน้าแรก)
    /// </summary>
    public class GetPublicPostsQuery : IRequest<GetPublicPostsResult>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? CommunityId { get; set; }
        public string? Tag { get; set; }
        public string SortBy { get; set; } = "PublishedAt"; // PublishedAt, ViewCount, LikeCount, Title
        public string SortDirection { get; set; } = "desc";
        public bool FeaturedOnly { get; set; } = false;
    }

    public class GetPublicPostsResult
    {
        public List<PublicPostItem> Posts { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class PublicPostItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Excerpt { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public DateTime PublishedAt { get; set; }
        public bool IsFeatured { get; set; }

        // Author Info
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfileImageUrl { get; set; }

        // Category & Community
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryColor { get; set; }
        public int? CommunityId { get; set; }
        public string? CommunityName { get; set; }
        public string? CommunityImageUrl { get; set; }

        // Stats
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }

        // Tags
        public List<string> Tags { get; set; } = new();

        // Computed Properties
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - PublishedAt;
                if (timeSpan.TotalMinutes < 1) return "เมื่อสักครู่";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} นาทีที่แล้ว";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} ชั่วโมงที่แล้ว";
                if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} วันที่แล้ว";
                if (timeSpan.TotalDays < 30) return $"{(int)(timeSpan.TotalDays / 7)} สัปดาห์ที่แล้ว";
                if (timeSpan.TotalDays < 365) return $"{(int)(timeSpan.TotalDays / 30)} เดือนที่แล้ว";
                return $"{(int)(timeSpan.TotalDays / 365)} ปีที่แล้ว";
            }
        }

        public string ReadingTime
        {
            get
            {
                // คำนวณเวลาอ่าน (สมมติ 200 คำ/นาที)
                var wordCount = Excerpt?.Split(' ').Length ?? 0;
                var minutes = Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));
                return $"{minutes} นาที";
            }
        }
    }
}